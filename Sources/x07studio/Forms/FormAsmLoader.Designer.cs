namespace x07studio.Forms
{
    partial class FormAsmLoader
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
            Label label1;
            HexTextBox = new TextBox();
            GenerateButton = new Button();
            label1 = new Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 10);
            label1.Name = "label1";
            label1.Size = new Size(223, 17);
            label1.TabIndex = 1;
            label1.Text = "Veuillez coller le code au format HEX";
            // 
            // HexTextBox
            // 
            HexTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            HexTextBox.BorderStyle = BorderStyle.FixedSingle;
            HexTextBox.Font = new Font("Consolas", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            HexTextBox.Location = new Point(12, 31);
            HexTextBox.Multiline = true;
            HexTextBox.Name = "HexTextBox";
            HexTextBox.Size = new Size(456, 424);
            HexTextBox.TabIndex = 0;
            // 
            // GenerateButton
            // 
            GenerateButton.Location = new Point(377, 461);
            GenerateButton.Name = "GenerateButton";
            GenerateButton.Size = new Size(91, 35);
            GenerateButton.TabIndex = 2;
            GenerateButton.Text = "Générer";
            GenerateButton.UseVisualStyleBackColor = true;
            GenerateButton.Click += GenerateButton_Click;
            // 
            // FormAsmLoader
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(480, 510);
            Controls.Add(GenerateButton);
            Controls.Add(label1);
            Controls.Add(HexTextBox);
            Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormAsmLoader";
            Text = "Chargeur de code machine";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox HexTextBox;
        private Button GenerateButton;
    }
}