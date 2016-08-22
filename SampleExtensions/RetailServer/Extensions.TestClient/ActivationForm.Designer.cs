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
        internal partial class ActivationForm
        {
            private System.Windows.Forms.Button btnActivate;
            private System.Windows.Forms.TextBox textBoxRetailServerUrl;
            private System.Windows.Forms.TextBox textBoxDeviceId;
            private System.Windows.Forms.TextBox textBoxRegisterId;
            private System.Windows.Forms.Label label1;
            private System.Windows.Forms.Label label2;
            private System.Windows.Forms.Label label3;

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
                this.btnActivate = new System.Windows.Forms.Button();
                this.textBoxRetailServerUrl = new System.Windows.Forms.TextBox();
                this.textBoxDeviceId = new System.Windows.Forms.TextBox();
                this.textBoxRegisterId = new System.Windows.Forms.TextBox();
                this.label1 = new System.Windows.Forms.Label();
                this.label2 = new System.Windows.Forms.Label();
                this.label3 = new System.Windows.Forms.Label();
                this.SuspendLayout();
                // 
                // btnActivate
                // 
                this.btnActivate.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
                this.btnActivate.Location = new System.Drawing.Point(16, 99);
                this.btnActivate.Name = "btnActivate";
                this.btnActivate.Size = new System.Drawing.Size(75, 23);
                this.btnActivate.TabIndex = 0;
                this.btnActivate.Text = "Activate";
                this.btnActivate.UseVisualStyleBackColor = true;
                this.btnActivate.Click += new System.EventHandler(this.btnActivate_Click);
                // 
                // textBoxRetailServerUrl
                // 
                this.textBoxRetailServerUrl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
                this.textBoxRetailServerUrl.Location = new System.Drawing.Point(106, 13);
                this.textBoxRetailServerUrl.Name = "textBoxRetailServerUrl";
                this.textBoxRetailServerUrl.ReadOnly = true;
                this.textBoxRetailServerUrl.Size = new System.Drawing.Size(512, 20);
                this.textBoxRetailServerUrl.TabIndex = 1;
                this.textBoxRetailServerUrl.Text = "https://usnconeboxax1ret.cloud.onebox.dynamics.com/RetailServer";
                // 
                // textBoxDeviceId
                // 
                this.textBoxDeviceId.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
                this.textBoxDeviceId.Location = new System.Drawing.Point(106, 39);
                this.textBoxDeviceId.Name = "textBoxDeviceId";
                this.textBoxDeviceId.Size = new System.Drawing.Size(512, 20);
                this.textBoxDeviceId.TabIndex = 2;
                this.textBoxDeviceId.Text = "HOUSTON-3";
                // 
                // textBoxRegisterId
                // 
                this.textBoxRegisterId.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
                this.textBoxRegisterId.Location = new System.Drawing.Point(106, 65);
                this.textBoxRegisterId.Name = "textBoxRegisterId";
                this.textBoxRegisterId.Size = new System.Drawing.Size(512, 20);
                this.textBoxRegisterId.TabIndex = 3;
                this.textBoxRegisterId.Text = "HOUSTON-3";
                // 
                // label1
                // 
                this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
                this.label1.AutoSize = true;
                this.label1.Location = new System.Drawing.Point(12, 16);
                this.label1.Name = "label1";
                this.label1.Size = new System.Drawing.Size(81, 13);
                this.label1.TabIndex = 4;
                this.label1.Text = "RetailServer Url";
                // 
                // label2
                // 
                this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
                this.label2.AutoSize = true;
                this.label2.Location = new System.Drawing.Point(13, 42);
                this.label2.Name = "label2";
                this.label2.Size = new System.Drawing.Size(53, 13);
                this.label2.TabIndex = 5;
                this.label2.Text = "Device Id";
                // 
                // label3
                // 
                this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
                this.label3.AutoSize = true;
                this.label3.Location = new System.Drawing.Point(13, 66);
                this.label3.Name = "label3";
                this.label3.Size = new System.Drawing.Size(58, 13);
                this.label3.TabIndex = 6;
                this.label3.Text = "Register Id";
                // 
                // ActivationForm
                // 
                this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
                this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
                this.ClientSize = new System.Drawing.Size(630, 137);
                this.Controls.Add(this.label3);
                this.Controls.Add(this.label2);
                this.Controls.Add(this.label1);
                this.Controls.Add(this.textBoxRegisterId);
                this.Controls.Add(this.textBoxDeviceId);
                this.Controls.Add(this.textBoxRetailServerUrl);
                this.Controls.Add(this.btnActivate);
                this.Name = "ActivationForm";
                this.Text = "ActivationForm";
                this.Load += new System.EventHandler(this.ActivationForm_Load);
                this.ResumeLayout(false);
                this.PerformLayout();

            }

            #endregion
        }
    }
}