﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace x07studio.Forms
{
    public partial class FormSettings : Form
    {
        public FormSettings()
        {
            InitializeComponent();
        }

        private void FormSettings_Load(object sender, EventArgs e)
        {
            PortNameTextBox.Text = Properties.Settings.Default.PortName;
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.PortName = PortNameTextBox.Text;
            Properties.Settings.Default.Save();
            DialogResult = DialogResult.OK;
        }
    }
} 
