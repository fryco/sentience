namespace WindowsApplication1
{
    partial class frmMain
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
            this.picRays = new System.Windows.Forms.PictureBox();
            this.txtMappingTime = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtPositionError = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtAngularError = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.picRays)).BeginInit();
            this.SuspendLayout();
            // 
            // picRays
            // 
            this.picRays.Location = new System.Drawing.Point(15, 12);
            this.picRays.Name = "picRays";
            this.picRays.Size = new System.Drawing.Size(769, 585);
            this.picRays.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picRays.TabIndex = 0;
            this.picRays.TabStop = false;
            // 
            // txtMappingTime
            // 
            this.txtMappingTime.Location = new System.Drawing.Point(105, 625);
            this.txtMappingTime.Name = "txtMappingTime";
            this.txtMappingTime.Size = new System.Drawing.Size(63, 20);
            this.txtMappingTime.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 625);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Mapping time";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(193, 625);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(94, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Position Error (mm)";
            // 
            // txtPositionError
            // 
            this.txtPositionError.Location = new System.Drawing.Point(293, 625);
            this.txtPositionError.Name = "txtPositionError";
            this.txtPositionError.Size = new System.Drawing.Size(63, 20);
            this.txtPositionError.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(379, 625);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(115, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Angular Error (degrees)";
            // 
            // txtAngularError
            // 
            this.txtAngularError.Location = new System.Drawing.Point(500, 625);
            this.txtAngularError.Name = "txtAngularError";
            this.txtAngularError.Size = new System.Drawing.Size(63, 20);
            this.txtAngularError.TabIndex = 5;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(791, 681);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtAngularError);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtPositionError);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtMappingTime);
            this.Controls.Add(this.picRays);
            this.Name = "frmMain";
            this.Text = "Sentience test";
            ((System.ComponentModel.ISupportInitialize)(this.picRays)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picRays;
        private System.Windows.Forms.TextBox txtMappingTime;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtPositionError;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtAngularError;
    }
}
