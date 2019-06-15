using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using Binarysharp.MemoryManagement;
using System.Diagnostics;

namespace UnistRecordingEditor
{
    public partial class mainForm : Form
    {
        private Dictionary<String, int> buttons = new Dictionary<string, int>() { { "A", 1 }, { "B", 2 }, { "C", 4 }, { "D", 8 } };
        private int dir_modifier = 0x100;
        private MemorySharp ms;
        private int[] slotoffsets = { 0x7402FC, 0x740FC8, 0x741C94, 0x742960 , 0x74362C };
        private IntPtr currentoffset = IntPtr.Zero;
        public mainForm()
        {
            InitializeComponent();
            var unistproc = Process.GetProcessesByName("UNIst").FirstOrDefault();
            if(unistproc == null)
            {
                MessageBox.Show("Unist Not Open!  Open the game and try again.");
                Application.Exit();
            }
            ms = new MemorySharp(unistproc);
        }
        private Boolean isValidSequence()
        {
            var inputLines = inputTextBox.Lines;
            foreach (var line in inputLines)
            {
                if (!Regex.IsMatch(line, "[1-9][A-Da-d]*,\\d+") && line != "")
                {
                    return false;
                }
            }
            return true;
        }
        private void inputTextBox_TextChanged(object sender, EventArgs e)
        {
            button1.Enabled = isValidSequence();
            if (isValidSequence())
            {
                var framect = 0;
                foreach (var line in inputTextBox.Lines)
                {
                    if (line != "")
                    {
                        var splitline = line.Split(',');
                        framect += int.Parse(splitline[1]);
                    }
                }
                label1.Text = "Length in Frames: " + framect.ToString();
            }
        }

        private void slotBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentoffset = (IntPtr)slotoffsets[slotBox.SelectedIndex];
        }

        private int[] parseInputs()
        {
            List<int> inputints = new List<int>();
            foreach (var line in inputTextBox.Lines)
            {
                var input = 0;
                var splitline = line.Split(',');
                var direction = int.Parse(splitline[0].First().ToString());
                if(direction != 5)
                {
                    input += direction * dir_modifier;
                }
                var buttonstr = splitline[0].Substring(1);
                foreach (var button in buttonstr)
                {
                    input |= buttons[button.ToString()];
                }
                var count = int.Parse(splitline[1]);
                for(var i = 0; i < count; i++)
                {
                    inputints.Add(input);
                }
            }
            while(inputints.Count < 500)
            {
                inputints.Add(0);
            }
            return inputints.ToArray();

        }


        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (isValidSequence() && currentoffset != IntPtr.Zero)
            {
                var inputs = parseInputs();
                ms.Write<int>(currentoffset, inputs);
            }
        }
    }
}
