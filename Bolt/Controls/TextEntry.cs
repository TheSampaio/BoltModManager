using System.ComponentModel;

namespace Bolt.Controls
{
    public partial class TextEntry : UserControl
    {
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Bindable(true)]
        [Category("Appearance")]
        [Description("The text associated with the \"Label\" inside the control.")]
        public override string Text
        {
            get => LblTextEntry?.Text ?? string.Empty;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CS8765  // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
            set
#pragma warning restore CS8765
#pragma warning restore IDE0079
            {
                if (LblTextEntry is not null)
                    LblTextEntry.Text = value;
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Bindable(true)]
        [Category("Appearance")]
        [Description("The text associated with the \"TextBox\" inside the control.")]
        public string Value
        {
            get => TxtTextEntry?.Text ?? string.Empty;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CS8765  // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
            set
#pragma warning restore CS8765
#pragma warning restore IDE0079
            {
                if (TxtTextEntry is not null)
                    TxtTextEntry.Text = value;
            }
        }

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Bindable(true)]
        [Category("Behavior")]
        [Description("Indicates whether the text box is read-only.")]
        public bool ReadOnly
        {
            get => TxtTextEntry?.ReadOnly ?? false;
            set
            {
                if (TxtTextEntry is not null)
                    TxtTextEntry.ReadOnly = value;
            }
        }

        public TextEntry()
        {
            InitializeComponent();

            // Sync initial value
            base.Text = LblTextEntry.Text;
        }
    }
}
