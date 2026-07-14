namespace CFTunnel
{
    // One Monokai palette
    internal static class Theme
    {
        public static readonly Color Back = Color.FromArgb(33, 37, 43);        // #21252B window / darkest
        public static readonly Color Panel = Color.FromArgb(40, 44, 52);       // #282C34 content
        public static readonly Color Field = Color.FromArgb(44, 49, 58);       // #2C313A inputs / buttons
        public static readonly Color Border = Color.FromArgb(24, 26, 31);      // #181A1F outlines
        public static readonly Color Fore = Color.FromArgb(220, 223, 228);     // #DCDFE4 main text
        public static readonly Color ForeDim = Color.FromArgb(171, 178, 191);  // #ABB2BF secondary text
        public static readonly Color Hint = Color.FromArgb(138, 145, 158);     // #8A919E comments / hints
        public static readonly Color Selection = Color.FromArgb(62, 68, 81);   // #3E4451 hover / active
        public static readonly Color Accent = Color.FromArgb(97, 175, 239);    // #61AFEF blue accent
        public static readonly Color Green = Color.FromArgb(152, 195, 121);    // #98C379 success / log
        public static readonly Color Red = Color.FromArgb(224, 108, 117);      // #E06C75 danger

        public static readonly Font UI = new("Segoe UI", 10f);
        public static readonly Font UIBold = new("Segoe UI", 10f, FontStyle.Bold);
        public static readonly Font Mono = new("Consolas", 10f);
        public static readonly Font MonoBold = new("Consolas", 10f, FontStyle.Bold);

        public static void StyleButton(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = Selection;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.MouseOverBackColor = Selection;
            btn.FlatAppearance.MouseDownBackColor = Border;
            btn.BackColor = Field;
            btn.ForeColor = Fore;
            btn.AutoSize = true;
            btn.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btn.Padding = new Padding(10, 4, 10, 4);
            btn.UseVisualStyleBackColor = false;
            // Disabled flat buttons render near-invisible gray text on dark; dim the surface instead.
            btn.EnabledChanged += (s, e) =>
            {
                btn.BackColor = btn.Enabled ? Field : Back;
                btn.FlatAppearance.BorderColor = btn.Enabled ? Selection : Border;
            };
        }

        // Accent-filled button for the main action of each page.
        public static void StylePrimaryButton(Button btn)
        {
            var darkText = Color.FromArgb(20, 23, 28);
            btn.BackColor = Accent;
            btn.ForeColor = darkText;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(130, 192, 245);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(72, 140, 200);
            btn.EnabledChanged += (s, e) =>
            {
                btn.BackColor = btn.Enabled ? Accent : Back;
                btn.ForeColor = btn.Enabled ? darkText : Hint;
            };
        }

        public static void StyleDangerButton(Button btn)
        {
            btn.ForeColor = Red;
            btn.FlatAppearance.BorderColor = Color.FromArgb(92, 58, 63);
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(74, 41, 46);
        }

        public static void StyleTextBox(TextBox txt)
        {
            txt.BackColor = Field;
            txt.ForeColor = Fore;
            txt.BorderStyle = BorderStyle.FixedSingle;
        }

        public static void StyleGrid(DataGridView grid)
        {
            grid.BackgroundColor = Panel;
            grid.BorderStyle = BorderStyle.FixedSingle;
            grid.GridColor = Selection;
            grid.EnableHeadersVisualStyles = false;
            grid.RowHeadersVisible = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Field;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Fore;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Field;
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
            grid.ColumnHeadersHeight = 32;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.DefaultCellStyle.BackColor = Panel;
            grid.DefaultCellStyle.ForeColor = Fore;
            grid.DefaultCellStyle.SelectionBackColor = Selection;
            grid.DefaultCellStyle.SelectionForeColor = Fore;
            grid.DefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
            grid.RowTemplate.Height = 28;
            grid.AllowUserToResizeRows = false;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        }
    }

    // Dark palette for ContextMenuStrip (tray menu).
    internal sealed class DarkMenuColors : ProfessionalColorTable
    {
        public override Color MenuItemSelected => Theme.Selection;
        public override Color MenuItemBorder => Theme.Selection;
        public override Color ToolStripDropDownBackground => Theme.Panel;
        public override Color ImageMarginGradientBegin => Theme.Panel;
        public override Color ImageMarginGradientMiddle => Theme.Panel;
        public override Color ImageMarginGradientEnd => Theme.Panel;
        public override Color MenuBorder => Theme.Border;
        public override Color SeparatorDark => Theme.Selection;
        public override Color SeparatorLight => Theme.Selection;
    }
}
