namespace Bolt
{
    public struct ButtonActionMap
    {
        public ButtonActionMap(string text, Action<string>? click = null)
        {
            Button = new Dictionary<string, Button?>
            {
                { text, null }
            };

            Click = click;
        }

        public Dictionary<string, Button?> Button;
        public Action<string>? Click;
    }

    public partial class FormMain : Form
    {
        private readonly List<ButtonActionMap> _sidePanelButtons;

        public FormMain()
        {
            InitializeComponent();

            _sidePanelButtons =
            [
                new("Games", Btn_Games_Click),
                new("Mods",  Btn_Mods_Click),
                new("Settings",  Btn_Settings_Click),
            ];
        }

        private void FormMain_Shown(object sender, EventArgs e)
        {
            foreach (var buttonAction in _sidePanelButtons)
            {
                Button buttonGeneric = new()
                {
                    Cursor = Cursors.Hand,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 14),
                    ForeColor = Color.White,
                    Margin = new Padding(0),
                    Size = new Size(200, 50),
                    Text = buttonAction.Button.Keys.FirstOrDefault(),
                    TextAlign = ContentAlignment.MiddleLeft,
                };

                buttonGeneric.FlatAppearance.BorderSize = 0;

                // Associate the click event for the button
                buttonGeneric.Click += (sender, e) =>
                {
                    if (buttonAction.Click != null && buttonGeneric.Text != null)
                        buttonAction.Click.Invoke(buttonGeneric.Text);
                };

                // Add the button to the flow panel
                FlpSideBar.Controls.Add(buttonGeneric);
            }
        }

        // Click handler for the "Games" button
        private void Btn_Games_Click(string text)
        {
            AttachFormInPanel(new FormGames());
        }

        // Click handler for the "Mods" button
        private void Btn_Mods_Click(string text)
        {
            AttachFormInPanel(new FormMods());
        }

        // Click handler for the "Settings" button
        private void Btn_Settings_Click(string text)
        {
            AttachFormInPanel(new FormSettings());
        }

        private Form AttachFormInPanel(Form form)
        {
            // Check if a form of the same type is already in the panel
            if (Pnl_Display.Controls.OfType<Form>().Any(existingForm => existingForm.GetType() == form.GetType()))
                return form;

            // Clear the panel before adding the new form
            Pnl_Display.Controls.Clear();

            // Configure the form instance
            form.Dock = DockStyle.Fill;
            form.FormBorderStyle = FormBorderStyle.None;
            form.TopLevel = false;

            // Add the form to the panel and show it
            Pnl_Display.Controls.Add(form);
            form.Show();

            return form;
        }
    }
}
