﻿namespace x07studio.Forms
{
    partial class FormDump
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            OutputTextBox = new TextBox();
            AddressTextBox = new TextBox();
            DumpButton = new Button();
            DumpSizeTextBox = new TextBox();
            TransferProgressBar = new ProgressBar();
            SuspendLayout();
            // 
            // OutputTextBox
            // 
            OutputTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            OutputTextBox.BorderStyle = BorderStyle.FixedSingle;
            OutputTextBox.Font = new Font("Consolas", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            OutputTextBox.Location = new Point(12, 12);
            OutputTextBox.Multiline = true;
            OutputTextBox.Name = "OutputTextBox";
            OutputTextBox.ReadOnly = true;
            OutputTextBox.ScrollBars = ScrollBars.Vertical;
            OutputTextBox.ShortcutsEnabled = false;
            OutputTextBox.Size = new Size(613, 436);
            OutputTextBox.TabIndex = 0;
            OutputTextBox.WordWrap = false;
            // 
            // AddressTextBox
            // 
            AddressTextBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            AddressTextBox.BorderStyle = BorderStyle.FixedSingle;
            AddressTextBox.Font = new Font("Consolas", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            AddressTextBox.Location = new Point(378, 471);
            AddressTextBox.Name = "AddressTextBox";
            AddressTextBox.Size = new Size(80, 23);
            AddressTextBox.TabIndex = 1;
            AddressTextBox.Text = "$3000";
            AddressTextBox.TextAlign = HorizontalAlignment.Center;
            // 
            // DumpButton
            // 
            DumpButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            DumpButton.Location = new Point(550, 471);
            DumpButton.Name = "DumpButton";
            DumpButton.Size = new Size(75, 23);
            DumpButton.TabIndex = 2;
            DumpButton.Text = "DUMP";
            DumpButton.UseVisualStyleBackColor = true;
            DumpButton.Click += DumpButton_Click;
            // 
            // DumpSizeTextBox
            // 
            DumpSizeTextBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            DumpSizeTextBox.BorderStyle = BorderStyle.FixedSingle;
            DumpSizeTextBox.Font = new Font("Consolas", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            DumpSizeTextBox.Location = new Point(464, 471);
            DumpSizeTextBox.Name = "DumpSizeTextBox";
            DumpSizeTextBox.Size = new Size(80, 23);
            DumpSizeTextBox.TabIndex = 3;
            DumpSizeTextBox.Text = "512";
            DumpSizeTextBox.TextAlign = HorizontalAlignment.Center;
            // 
            // TransferProgressBar
            // 
            TransferProgressBar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            TransferProgressBar.Location = new Point(12, 471);
            TransferProgressBar.Name = "TransferProgressBar";
            TransferProgressBar.Size = new Size(201, 23);
            TransferProgressBar.TabIndex = 4;
            // 
            // FormDump
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(637, 505);
            Controls.Add(TransferProgressBar);
            Controls.Add(DumpSizeTextBox);
            Controls.Add(DumpButton);
            Controls.Add(AddressTextBox);
            Controls.Add(OutputTextBox);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormDump";
            Text = "Memory manager";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox OutputTextBox;
        private TextBox AddressTextBox;
        private Button DumpButton;
        private TextBox DumpSizeTextBox;
        private ProgressBar TransferProgressBar;
    }
}