namespace x07studio.Forms
{
    partial class FormTransfert
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormTransfert));
            MessageLabel = new Label();
            StartButton = new Button();
            CancelButton = new Button();
            TransfertProgress = new ProgressBar();
            pictureBox3 = new PictureBox();
            pictureBox2 = new PictureBox();
            pictureBox1 = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // MessageLabel
            // 
            MessageLabel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            MessageLabel.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            MessageLabel.Location = new Point(17, 190);
            MessageLabel.Margin = new Padding(4, 0, 4, 0);
            MessageLabel.Name = "MessageLabel";
            MessageLabel.Size = new Size(584, 110);
            MessageLabel.TabIndex = 0;
            MessageLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // StartButton
            // 
            StartButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            StartButton.Location = new Point(474, 313);
            StartButton.Margin = new Padding(4, 5, 4, 5);
            StartButton.Name = "StartButton";
            StartButton.Size = new Size(127, 47);
            StartButton.TabIndex = 1;
            StartButton.Text = "Démarrer";
            StartButton.UseVisualStyleBackColor = true;
            StartButton.Click += StartButton_Click;
            // 
            // CancelButton
            // 
            CancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            CancelButton.Location = new Point(474, 313);
            CancelButton.Margin = new Padding(4, 5, 4, 5);
            CancelButton.Name = "CancelButton";
            CancelButton.Size = new Size(127, 47);
            CancelButton.TabIndex = 2;
            CancelButton.Text = "Annuler";
            CancelButton.UseVisualStyleBackColor = true;
            CancelButton.Visible = false;
            CancelButton.Click += CancelButton_Click;
            // 
            // TransfertProgress
            // 
            TransfertProgress.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            TransfertProgress.BackColor = SystemColors.Control;
            TransfertProgress.Location = new Point(17, 318);
            TransfertProgress.Margin = new Padding(4, 5, 4, 5);
            TransfertProgress.Name = "TransfertProgress";
            TransfertProgress.Size = new Size(449, 38);
            TransfertProgress.TabIndex = 3;
            TransfertProgress.Visible = false;
            // 
            // pictureBox3
            // 
            pictureBox3.Image = (Image)resources.GetObject("pictureBox3.Image");
            pictureBox3.Location = new Point(234, 20);
            pictureBox3.Margin = new Padding(4, 5, 4, 5);
            pictureBox3.Name = "pictureBox3";
            pictureBox3.Size = new Size(154, 148);
            pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox3.TabIndex = 9;
            pictureBox3.TabStop = false;
            // 
            // pictureBox2
            // 
            pictureBox2.Image = (Image)resources.GetObject("pictureBox2.Image");
            pictureBox2.Location = new Point(17, 20);
            pictureBox2.Margin = new Padding(4, 5, 4, 5);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(209, 148);
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.TabIndex = 8;
            pictureBox2.TabStop = false;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(397, 20);
            pictureBox1.Margin = new Padding(4, 5, 4, 5);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(209, 148);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 7;
            pictureBox1.TabStop = false;
            // 
            // FormTransfert
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(619, 372);
            Controls.Add(pictureBox3);
            Controls.Add(pictureBox2);
            Controls.Add(pictureBox1);
            Controls.Add(TransfertProgress);
            Controls.Add(CancelButton);
            Controls.Add(StartButton);
            Controls.Add(MessageLabel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(4, 5, 4, 5);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FormTransfert";
            StartPosition = FormStartPosition.CenterParent;
            Text = "TRANSFERT";
            FormClosing += FormTransfert_FormClosing;
            Load += FormTransfert_Load;
            Shown += FormTransfert_Shown;
            ((System.ComponentModel.ISupportInitialize)pictureBox3).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Label MessageLabel;
        private Button StartButton;
        private Button CancelButton;
        private ProgressBar TransfertProgress;
        private PictureBox pictureBox3;
        private PictureBox pictureBox2;
        private PictureBox pictureBox1;
    }
}