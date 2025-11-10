namespace Bolsover
{
    partial class StlStpForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StlStpForm));
            this.stlStpControl1 = new Bolsover.StlStpControl();
            this.SuspendLayout();
            // 
            // stlStpControl1
            // 
            this.stlStpControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.stlStpControl1.Location = new System.Drawing.Point(0, 0);
            this.stlStpControl1.Name = "stlStpControl1";
            this.stlStpControl1.Size = new System.Drawing.Size(850, 231);
            this.stlStpControl1.TabIndex = 0;
            // 
            // StlStpForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(850, 231);
            this.Controls.Add(this.stlStpControl1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "StlStpForm";
            this.Text = "Stl->Stp Converter";
            this.ResumeLayout(false);
        }

        private Bolsover.StlStpControl stlStpControl1;

        #endregion
    }
}