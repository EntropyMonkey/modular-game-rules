using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squid;

public class SampleWindow : Window
{
    public TitleBar Titlebar { get; private set; }

    public SampleWindow()
    {
        AllowDragOut = true;
        Padding = new Margin(4);

        Titlebar = new TitleBar();
        Titlebar.Dock = DockStyle.Top;
        Titlebar.Size = new Squid.Point(122, 32);
        Titlebar.MouseDown += delegate(Control sender, MouseEventArgs args) { StartDrag(); };
        Titlebar.MouseUp += delegate(Control sender, MouseEventArgs args) { StopDrag(); };
        Titlebar.Cursor = Cursors.Move;
        Titlebar.Style = "titlebar";
        //Titlebar.Margin = new Margin(-4, -4, -4, 0);
        Titlebar.TextAlign = Alignment.MiddleLeft;
        Titlebar.BBCodeEnabled = true;
        Titlebar.Button.MouseClick += Button_OnMouseClick;
        Controls.Add(Titlebar);

        AllowDragOut = false;
    }

    void Button_OnMouseClick(Control sender, MouseEventArgs args)
    {
        Animation.Custom(FadeAndClose());
    }

    private System.Collections.IEnumerator FadeAndClose()
    {
        Enabled = false;

        yield return Animation.Opacity(0, 300);

        Close();
    }
}

public class TitleBar : Label
{
    public Button Button { get; private set; }

    public TitleBar()
    {
        Button = new Button();
        Button.Size = new Point(20, 20);
        Button.Style = "button";
        Button.Tooltip = "Close Window";
        Button.Dock = DockStyle.Right;
        Button.Margin = new Margin(0, 6, 6, 6);
        Elements.Add(Button);
    }
}

