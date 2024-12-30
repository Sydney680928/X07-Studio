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
            MessageLabel.Location = new Point(12, 114);
            MessageLabel.Name = "MessageLabel";
            MessageLabel.Size = new Size(409, 66);
            MessageLabel.TabIndex = 0;
            MessageLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // StartButton
            // 
            StartButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            StartButton.Location = new Point(332, 188);
            StartButton.Name = "StartButton";
            StartButton.Size = new Size(89, 28);
            StartButton.TabIndex = 1;
            StartButton.Text = "Démarrer";
            StartButton.UseVisualStyleBackColor = true;
            StartButton.Click += StartButton_Click;
            // 
            // CancelButton
            // 
            CancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            CancelButton.Location = new Point(332, 188);
            CancelButton.Name = "CancelButton";
            CancelButton.Size = new Size(89, 28);
            CancelButton.TabIndex = 2;
            CancelButton.Text = "Annuler";
            CancelButton.UseVisualStyleBackColor = true;
            CancelButton.Visible = false;
            CancelButton.Click += CancelButton_Click;
            // 
            // TransfertProgress
            // 
            TransfertProgress.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            TransfertProgress.Location = new Point(12, 191);
            TransfertProgress.Name = "TransfertProgress";
            TransfertProgress.Size = new Size(314, 23);
            TransfertProgress.TabIndex = 3;
            TransfertProgress.Visible = false;
            // 
            // pictureBox3
            // 
            pictureBox3.Image = (Image)resources.GetObject("pictureBox3.Image");
            pictureBox3.Location = new Point(164, 12);
            pictureBox3.Name = "pictureBox3";
            pictureBox3.Size = new Size(108, 89);
            pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox3.TabIndex = 9;
            pictureBox3.TabStop = false;
            // 
            // pictureBox2
            // 
            pictureBox2.Image = (Image)resources.GetObject("pictureBox2.Image");
            pictureBox2.Location = new Point(12, 12);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(146, 89);
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.TabIndex = 8;
            pictureBox2.TabStop = false;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(278, 12);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(146, 89);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 7;
            pictureBox1.TabStop = false;
            // 
            // FormTransfert
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(433, 223);
            Controls.Add(pictureBox3);
            Controls.Add(pictureBox2);
            Controls.Add(pictureBox1);
            Controls.Add(TransfertProgress);
            Controls.Add(CancelButton);
            Controls.Add(StartButton);
            Controls.Add(MessageLabel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
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