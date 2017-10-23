using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Xml;
using System.Drawing;

namespace DMXtable
{
    public class FixturesAndChannels
    {
        public byte[] byteArray { get; set; }
        public MyColor[] fixtureColors { get; set; }
    }

    class DMXmaster
    {
        public List<Fixture> fixtures;
        private Effect effect;
        private int channelCount;
        private int mainFade = 1000;
        private double mainMaster = 1;

        private Stopwatch stopwatch;
        private long lastMilliseconds;
        private double millPerFrame;

        private string effectGroup = "Effect";

        public DMXmaster()
        {
            fixtures = new List<Fixture>();
            stopwatch = new Stopwatch();
            stopwatch.Start();
            effect = new Effect(fixtures.Count);
        }

        public int update()
        {
            foreach (Fixture fix in fixtures)
            {
                fix.fade();
            }

            effect.update(mainFade);

            millPerFrame += ((double)(stopwatch.ElapsedMilliseconds - lastMilliseconds) - millPerFrame) * 0.03;
            lastMilliseconds = stopwatch.ElapsedMilliseconds;

            return 1000 / (int)millPerFrame;
        }

        public void addFixtures(string name, int nChannels, int startChannel, string groups, int repeat = 1)
        {
            int s = startChannel;
            for (int i = 0; i < repeat; i++)
            {
                fixtures.Add(new Fixture(name, nChannels, s, groups));
                s += nChannels;
            }

            recountChannels();
        }

        public void updateFixtures(string name, string values)
        {
            System.Drawing.Color color = ColorTranslator.FromHtml(values);
            byte[] channels = new byte[3] { color.R, color.G, color.B };

            foreach (Fixture fix in fixtures)
            {
                if (fix.name == name || fix.isInGroup(name))
                    fix.updateChannels(channels, mainFade/millPerFrame);
            }
        }

        public void updateMainMaster(double val)
        {
            mainMaster = val;
        }

        public void updateEffectSelect(int tag)
        {
            effect.currentEffect = (Effect.Effects)tag;
        }

        public void updateEffectAppearance(bool tag)
        {
            effect.onOff = tag;
        }

        public void updateEffectColor(string val)
        {
            System.Drawing.Color color = ColorTranslator.FromHtml(val);
            byte[] channels = new byte[3] { color.R, color.G, color.B };
            effect.channels = channels;
        }

        public void updateEffectMaster(double master)
        {
            effect.master = master;
        }

        public void updateEffectOnOffTreshold(double val)
        {
            effect.onOffTreshold = val;
        }

        private void recountChannels()
        {
            channelCount = 0;

            foreach (Fixture fix in fixtures)
            {
                channelCount += fix.nChannels;
            }

            effect.size = fixtures.Count;
        }

        public FixturesAndChannels getFixtureArray()
        {
            FixturesAndChannels f = new FixturesAndChannels();

            f.byteArray = new byte[channelCount];
            f.fixtureColors = new MyColor[fixtures.Count];

            int c = -1; // total channel count
            int t = 0; // total light count
            int e = 0; // effect light count

            foreach (Fixture fix in fixtures)
            {
                byte[] result = (byte[])fix.currentChannels.Clone();
                if (fix.isInGroup(effectGroup))
                {
                    byte[] mix = effect.mix(result, e, mainMaster);
                    
                    Array.Copy(mix, 0, result, 0, mix.Length);

                    e++;
                }                

                for (int i = 0; i < result.Length; i++)
                {
                    f.byteArray[++c] = result[i];
                }

                f.fixtureColors[t] = new MyColor(result);
                t++;
            }

            return f;
        }

        public XmlElement getXML(XmlDocument doc)
        {
            XmlElement xml = doc.CreateElement("fixtures");
            foreach (Fixture fix in fixtures)
            {
                XmlElement xmlFix = doc.CreateElement("fixture");

                XmlAttribute attribute = doc.CreateAttribute("name");
                attribute.Value = fix.name;
                xmlFix.Attributes.Append(attribute);

                attribute = doc.CreateAttribute("nChannels");
                attribute.Value = fix.nChannels.ToString();
                xmlFix.Attributes.Append(attribute);

                attribute = doc.CreateAttribute("startAddr");
                attribute.Value = fix.startAddr.ToString();
                xmlFix.Attributes.Append(attribute);

                attribute = doc.CreateAttribute("groups");
                attribute.Value = fix.memberOfGroups;
                xmlFix.Attributes.Append(attribute);

                xml.AppendChild(xmlFix);
            }

            return xml;
        }

        public void loadXML(XmlDocument doc)
        {
            fixtures.Clear();
            XmlNode root = doc.SelectSingleNode("fixtures");
            if (root.HasChildNodes)
            {
                for (int i = 0; i < root.ChildNodes.Count; i++)
                {
                    XmlElement fix = (XmlElement)root.ChildNodes[i];
                    string name = fix.GetAttributeNode("name").InnerXml;
                    int nChannels = Int32.Parse(fix.GetAttributeNode("nChannels").InnerXml);
                    int StartAddr = Int32.Parse(fix.GetAttributeNode("startAddr").InnerXml);
                    string groups = fix.GetAttributeNode("groups").InnerXml;
                    addFixtures(name, nChannels, StartAddr, groups);
                }
            }
        }
    }
}
