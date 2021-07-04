namespace TurboCup
{
    partial class Uploader
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Uploader));
            this.pbRecords = new System.Windows.Forms.ProgressBar();
            this.lblProgress = new System.Windows.Forms.Label();
            this.lblRecords = new System.Windows.Forms.Label();
            this.lblUser = new System.Windows.Forms.Label();
            this.btnStart = new System.Windows.Forms.Button();
            this.lblFilename = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // pbRecords
            // 
            this.pbRecords.Location = new System.Drawing.Point(12, 28);
            this.pbRecords.Name = "pbRecords";
            this.pbRecords.Size = new System.Drawing.Size(443, 33);
            this.pbRecords.TabIndex = 0;
            // 
            // lblProgress
            // 
            this.lblProgress.Location = new System.Drawing.Point(12, 67);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new System.Drawing.Size(132, 23);
            this.lblProgress.TabIndex = 1;
            this.lblProgress.Text = "Progress: 0 %";
            this.lblProgress.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblRecords
            // 
            this.lblRecords.Location = new System.Drawing.Point(279, 4);
            this.lblRecords.Name = "lblRecords";
            this.lblRecords.Size = new System.Drawing.Size(176, 21);
            this.lblRecords.TabIndex = 2;
            this.lblRecords.Text = "Record 0 of 0";
            this.lblRecords.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblUser
            // 
            this.lblUser.Location = new System.Drawing.Point(12, 4);
            this.lblUser.Name = "lblUser";
            this.lblUser.Size = new System.Drawing.Size(261, 21);
            this.lblUser.TabIndex = 3;
            this.lblUser.Text = "Current User:";
            this.lblUser.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(373, 67);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(82, 23);
            this.btnStart.TabIndex = 4;
            this.btnStart.Text = "Start Upload";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // lblFilename
            // 
            this.lblFilename.Location = new System.Drawing.Point(150, 67);
            this.lblFilename.Name = "lblFilename";
            this.lblFilename.Size = new System.Drawing.Size(217, 23);
            this.lblFilename.TabIndex = 5;
            this.lblFilename.Text = "Filename";
            this.lblFilename.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // Uploader
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(467, 99);
            this.Controls.Add(this.lblFilename);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.lblUser);
            this.Controls.Add(this.lblRecords);
            this.Controls.Add(this.lblProgress);
            this.Controls.Add(this.pbRecords);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Uploader";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Uploader";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ProgressBar pbRecords;
        private System.Windows.Forms.Label lblProgress;
        private System.Windows.Forms.Label lblRecords;
        private System.Windows.Forms.Label lblUser;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Label lblFilename;
    }
}