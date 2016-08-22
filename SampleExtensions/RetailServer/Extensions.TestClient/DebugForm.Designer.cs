/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

namespace Contoso
{
    namespace RetailServer.TestClient
    {
        internal partial class DebugForm
        {
            private System.Windows.Forms.TextBox textBoxDebug;

            /// <summary>
            /// Required designer variable.
            /// </summary>
            private System.ComponentModel.IContainer components = null;

            /// <summary>
            /// Clean up any resources being used.
            /// </summary>
            /// <param name="disposing">True if managed resources should be disposed; otherwise, false.</param>
            protected override void Dispose(bool disposing)
            {
                if (disposing && (this.components != null))
                {
                    this.components.Dispose();
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
                this.textBoxDebug = new System.Windows.Forms.TextBox();
                this.SuspendLayout();
                // 
                // textBoxDebug
                // 
                this.textBoxDebug.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
                this.textBoxDebug.Location = new System.Drawing.Point(12, 12);
                this.textBoxDebug.Multiline = true;
                this.textBoxDebug.Name = "textBoxDebug";
                this.textBoxDebug.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
                this.textBoxDebug.Size = new System.Drawing.Size(615, 456);
                this.textBoxDebug.TabIndex = 0;
                // 
                // DebugForm
                // 
                this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
                this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
                this.ClientSize = new System.Drawing.Size(639, 480);
                this.Controls.Add(this.textBoxDebug);
                this.Name = "DebugForm";
                this.Text = "DebugForm";
                this.ResumeLayout(false);
                this.PerformLayout();

            }

            #endregion
        }
    }
}