using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Xml;
using System.Timers;

namespace DMXtable
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DMXserial serial;
        DMXmaster master;
        System.Timers.Timer timer;
        BrushConverter converter;

        public MainWindow()
        {
            InitializeComponent();

            serial = new DMXserial();
            populatePortList();

            master = new DMXmaster();

            timer = new System.Timers.Timer();
            timer.Interval = 10;
            timer.Elapsed += mainLoop;

            converter = new BrushConverter();

            FIXTURE_list.ItemsSource = master.fixtures;

            loadXMLFile(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)+"\\default.xml");
        }

        private void mainLoop(Object source, ElapsedEventArgs e)
        {
            if (!serial.isConnected)
            {
                timer.Stop();
                return;
            }

            int fps = master.update();

            FixturesAndChannels f = master.getFixtureArray();

            this.Dispatcher.Invoke(() =>
            {
                FPSmeter.Content = fps + " fps";

                int i = 0;
                foreach (MyColor c in f.fixtureColors)
                {
                    Console.WriteLine(c.Hex);
                    (FIXTURE_preview.Children[i] as Ellipse).Fill = (Brush)converter.ConvertFromString(c.Hex);
                    i++;
                }
            });            

            serial.send(f.byteArray);

        }

        private void FixtureEditor_updateUI(object sender, RoutedEventArgs e)
        {
            FixtureEditor_updateUI();
        }

        private void FixtureEditor_updateUI()
        {
            this.Dispatcher.Invoke(() =>
            {
                FIXTURE_list.Items.Refresh();

                FIXTURE_preview.Children.Clear();

                for (int i = 0; i < master.fixtures.Count; i++) {
                    Ellipse myEllipse = new Ellipse();
                    myEllipse.Stroke = System.Windows.Media.Brushes.White;
                    myEllipse.Fill = System.Windows.Media.Brushes.Black;
                    myEllipse.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                    myEllipse.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    myEllipse.Width = 40;
                    myEllipse.Height = 40;
                    myEllipse.Margin = new System.Windows.Thickness(5);
                    FIXTURE_preview.Children.Add(myEllipse);
                }
            });
        }

        private void populatePortList() {
            string[] ports = serial.ports();

            foreach (string port in ports)
            {
                COMselect.Items.Add(port);
            }
            COMselect.SelectedIndex = 2;
        }

        private void COMconnect_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(COMselect.SelectedItem);
            if (serial.connect(COMselect.SelectedItem.ToString())) {
                COMconnect.Background = Brushes.Green;
                timer.Start();
            }
            else {
                COMconnect.Background = Brushes.Red;
            }
        }

        private void FIXcreate_Click(object sender, RoutedEventArgs e)
        {
            master.addFixtures(
                    (NewFixtureName as System.Windows.Controls.TextBox).Text,
                    Int32.Parse((NewFixtureNChannels as System.Windows.Controls.TextBox).Text),
                    Int32.Parse((NewFixtureStart as System.Windows.Controls.TextBox).Text),
                    (NewFixtureGroups as System.Windows.Controls.TextBox).Text,
                    Int32.Parse((NewFixtureRepeat as System.Windows.Controls.TextBox).Text)
                );

            FixtureEditor_updateUI();
        }

        private void SETsave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();

            sfd.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            sfd.FileName = "default.xml";
            sfd.FilterIndex = 2;
            sfd.CheckPathExists = true;
            sfd.RestoreDirectory = true;

            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                XmlDocument xdoc = new XmlDocument();
                xdoc.AppendChild(master.getXML(xdoc));
                xdoc.Save(sfd.FileName);
            }
        }

        private void SETload_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            ofd.FilterIndex = 2;
            ofd.RestoreDirectory = true;

            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                loadXMLFile(ofd.FileName);
            }
        }

        private void MAINmaster_Change(object sender, RoutedEventArgs e)
        {
            if (master != null)
                master.updateMainMaster((sender as Slider).Value);
        }

        private void FXmaster_Change(object sender, RoutedEventArgs e)
        {
            master.updateEffectMaster((FXmaster as Slider).Value);
        }

        private void FXselect_Click(object sender, RoutedEventArgs e)
        {
            int tag = Int32.Parse((sender as System.Windows.Controls.RadioButton).Tag.ToString());
            master.updateEffectSelect(tag);
        }

        private void FXappearance_Click(object sender, RoutedEventArgs e)
        {
            bool tag = (sender as System.Windows.Controls.Button).Tag.ToString() == "OnOff";
            master.updateEffectAppearance(tag);
        }

        private void FXtreshold_Change(object sender, RoutedEventArgs e)
        {
            if (master == null) return;

            double val = (sender as Slider).Value;
            FXtreshold_display.Content = Decimal.Round((decimal)val,2).ToString();
            master.updateEffectOnOffTreshold(val);
        }

        private void loadXMLFile(string fileName)
        {
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(fileName);
            master.loadXML(xdoc);

            FixtureEditor_updateUI();
        }

        private void Color_Click(object sender, RoutedEventArgs e)
        {
            string[] tags = (sender as System.Windows.Controls.Button).Tag.ToString().Split(',');

            master.updateFixtures(tags[0], tags[1]);
        }

        private void Intensity_Click(object sender, RoutedEventArgs e)
        {
            string[] tags = (sender as System.Windows.Controls.Button).Tag.ToString().Split(',');

            master.updateFixtures(tags[0], tags[1]);
        }

        private void Effect_Color_Click(object sender, RoutedEventArgs e)
        {
            master.updateEffectColor((sender as System.Windows.Controls.Button).Tag.ToString());
        }
    }
}
