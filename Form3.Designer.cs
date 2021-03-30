namespace laba3
{
    partial class Form3
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
            this.Continue = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // Continue
            // 
            this.Continue.Location = new System.Drawing.Point(12, 58);
            this.Continue.Name = "Continue";
            this.Continue.Size = new System.Drawing.Size(235, 29);
            this.Continue.TabIndex = 0;
            this.Continue.Text = "Continue execution";
            this.Continue.UseVisualStyleBackColor = true;
            this.Continue.Click += new System.EventHandler(this.Continue_Click);
            // 
            // Cancel
            // 
            this.Cancel.Location = new System.Drawing.Point(269, 58);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(107, 29);
            this.Cancel.TabIndex = 1;
            this.Cancel.Text = "Cancel the task";
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(338, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "A task, that has exceeded its execution timeout, has been suspended.";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 31);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(119, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Choose the next action!";
            // 
            // Form3
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(391, 97);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.Continue);
            this.Name = "Form3";
            this.Text = "Action for the task expected";
            this.Load += new System.EventHandler(this.Form3_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Continue;
        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}