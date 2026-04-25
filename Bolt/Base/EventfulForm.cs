namespace Bolt.Base
{
    public class EventfulForm : Form
    {
        protected EventfulForm() { }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            InitializeEvents();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                TerminateEvents();

            base.Dispose(disposing);
        }

        protected virtual void InitializeEvents() { }
        protected virtual void TerminateEvents() { }
    }
}