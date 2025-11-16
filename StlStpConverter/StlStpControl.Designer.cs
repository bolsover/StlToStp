using System.ComponentModel;

namespace Bolsover
{
    partial class StlStpControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.infile = new System.Windows.Forms.TextBox();
            this.outfile = new System.Windows.Forms.TextBox();
            this.inputButton = new System.Windows.Forms.Button();
            this.outputButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.message = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBox = new System.Windows.Forms.ComboBox();
            this.openChkBox = new System.Windows.Forms.CheckBox();
            this.splitCheckBox = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // infile
            // 
            this.infile.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.tableLayoutPanel1.SetColumnSpan(this.infile, 2);
            this.infile.Enabled = false;
            this.infile.Location = new System.Drawing.Point(8, 12);
            this.infile.Name = "infile";
            this.infile.Size = new System.Drawing.Size(644, 22);
            this.infile.TabIndex = 0;
            // 
            // outfile
            // 
            this.outfile.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.tableLayoutPanel1.SetColumnSpan(this.outfile, 2);
            this.outfile.Enabled = false;
            this.outfile.Location = new System.Drawing.Point(8, 48);
            this.outfile.Name = "outfile";
            this.outfile.Size = new System.Drawing.Size(644, 22);
            this.outfile.TabIndex = 1;
            // 
            // inputButton
            // 
            this.inputButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputButton.Location = new System.Drawing.Point(658, 8);
            this.inputButton.Name = "inputButton";
            this.inputButton.Size = new System.Drawing.Size(247, 30);
            this.inputButton.TabIndex = 2;
            this.inputButton.Text = "STL";
            this.inputButton.UseVisualStyleBackColor = true;
            // 
            // outputButton
            // 
            this.outputButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.outputButton.Location = new System.Drawing.Point(658, 44);
            this.outputButton.Name = "outputButton";
            this.outputButton.Size = new System.Drawing.Size(247, 30);
            this.outputButton.TabIndex = 3;
            this.outputButton.Text = "STP";
            this.outputButton.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 400F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 250F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 253F));
            this.tableLayoutPanel1.Controls.Add(this.infile, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.outputButton, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.outfile, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.inputButton, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.cancelButton, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.okButton, 2, 5);
            this.tableLayoutPanel1.Controls.Add(this.progressBar, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.message, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.label2, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.comboBox, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.openChkBox, 2, 4);
            this.tableLayoutPanel1.Controls.Add(this.splitCheckBox, 1, 4);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(5);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(5);
            this.tableLayoutPanel1.RowCount = 7;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(913, 233);
            this.tableLayoutPanel1.TabIndex = 4;
            // 
            // cancelButton
            // 
            this.cancelButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cancelButton.Location = new System.Drawing.Point(408, 188);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(244, 30);
            this.cancelButton.TabIndex = 4;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.okButton.Location = new System.Drawing.Point(658, 188);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(247, 30);
            this.okButton.TabIndex = 5;
            this.okButton.Text = "Convert";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // progressBar
            // 
            this.progressBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.progressBar.Location = new System.Drawing.Point(8, 188);
            this.progressBar.MarqueeAnimationSpeed = 30;
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(394, 30);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBar.TabIndex = 8;
            this.progressBar.Visible = false;
            // 
            // message
            // 
            this.message.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.tableLayoutPanel1.SetColumnSpan(this.message, 3);
            this.message.Location = new System.Drawing.Point(8, 116);
            this.message.Name = "message";
            this.message.Size = new System.Drawing.Size(794, 29);
            this.message.TabIndex = 7;
            this.message.Text = "Message";
            this.message.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.tableLayoutPanel1.SetColumnSpan(this.label2, 2);
            this.label2.Location = new System.Drawing.Point(408, 83);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(208, 23);
            this.label2.TabIndex = 10;
            this.label2.Text = "Tolerance";
            // 
            // comboBox
            // 
            this.comboBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.comboBox.FormattingEnabled = true;
            this.comboBox.Items.AddRange(new object[] { "1", "0.1", "0.01", "0.001", "0.0001", "0.00001", "0.000001", "0.0000001" });
            this.comboBox.Location = new System.Drawing.Point(8, 83);
            this.comboBox.Name = "comboBox";
            this.comboBox.Size = new System.Drawing.Size(394, 24);
            this.comboBox.TabIndex = 11;
            // 
            // openChkBox
            // 
            this.openChkBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.openChkBox.Location = new System.Drawing.Point(658, 152);
            this.openChkBox.Name = "openChkBox";
            this.openChkBox.Size = new System.Drawing.Size(247, 30);
            this.openChkBox.TabIndex = 6;
            this.openChkBox.Text = "Open STP after conversion";
            this.openChkBox.UseVisualStyleBackColor = true;
            // 
            // splitCheckBox
            // 
            this.splitCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitCheckBox.Location = new System.Drawing.Point(408, 152);
            this.splitCheckBox.Name = "splitCheckBox";
            this.splitCheckBox.Size = new System.Drawing.Size(244, 30);
            this.splitCheckBox.TabIndex = 12;
            this.splitCheckBox.Text = "Split STL before Converting";
            this.splitCheckBox.UseVisualStyleBackColor = true;
            // 
            // StlStpControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "StlStpControl";
            this.Size = new System.Drawing.Size(913, 233);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.CheckBox splitCheckBox;

        private System.Windows.Forms.ComboBox comboBox;

        private System.Windows.Forms.Label label2;

        private System.Windows.Forms.ProgressBar progressBar;

        private System.Windows.Forms.Label message;

        private System.Windows.Forms.CheckBox openChkBox;

        private System.Windows.Forms.Button okButton;

        private System.Windows.Forms.Button cancelButton;

        private System.Windows.Forms.TextBox infile;
        private System.Windows.Forms.TextBox outfile;
        private System.Windows.Forms.Button inputButton;
        private System.Windows.Forms.Button outputButton;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;

        #endregion
    }
}