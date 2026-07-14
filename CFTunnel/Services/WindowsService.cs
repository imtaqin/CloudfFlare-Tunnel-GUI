using System.ServiceProcess;

namespace CFTunnel.Services
{
    public static class WindowsService
    {
        public const string ServiceName = "cloudflared";

        public static ServiceControllerStatus? GetStatus()
        {
            try
            {
                using var controller = new ServiceController(ServiceName);
                return controller.Status;
            }
            catch
            {
                return null;
            }
        }

        public static async Task RestartAsync(CancellationToken cancellationToken = default)
        {
            using var controller = new ServiceController(ServiceName);
            await StopAsync(controller, cancellationToken);
            await StartAsync(controller, cancellationToken);
        }

        public static async Task StartAsync(CancellationToken cancellationToken = default)
        {
            using var controller = new ServiceController(ServiceName);
            await StartAsync(controller, cancellationToken);
        }

        public static async Task StopAsync(CancellationToken cancellationToken = default)
        {
            using var controller = new ServiceController(ServiceName);
            await StopAsync(controller, cancellationToken);
        }

        private static async Task StartAsync(ServiceController controller, CancellationToken cancellationToken)
        {
            controller.Refresh();
            if (controller.Status == ServiceControllerStatus.Running)
                return;

            controller.Start();
            var deadline = DateTime.Now.AddSeconds(60);
            while (controller.Status != ServiceControllerStatus.Running && DateTime.Now < deadline)
            {
                await Task.Delay(500, cancellationToken);
                controller.Refresh();
                if (controller.Status == ServiceControllerStatus.Stopped)
                    break;
            }

            if (controller.Status != ServiceControllerStatus.Running)
                throw new System.TimeoutException($"Service '{ServiceName}' did not start within 60 seconds.");
        }

        private static async Task StopAsync(ServiceController controller, CancellationToken cancellationToken)
        {
            controller.Refresh();
            if (controller.Status == ServiceControllerStatus.Stopped)
                return;

            if (controller.Status == ServiceControllerStatus.Running)
                controller.Stop();

            var deadline = DateTime.Now.AddSeconds(60);
            while (controller.Status != ServiceControllerStatus.Stopped && DateTime.Now < deadline)
            {
                await Task.Delay(500, cancellationToken);
                controller.Refresh();
            }

            if (controller.Status != ServiceControllerStatus.Stopped)
                throw new System.TimeoutException($"Service '{ServiceName}' did not stop within 60 seconds.");
        }
    }
}
