/**
 * SAMPLE CODE NOTICE
 * 
 * THIS SAMPLE CODE IS MADE AVAILABLE AS IS.  MICROSOFT MAKES NO WARRANTIES, WHETHER EXPRESS OR IMPLIED,
 * OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY OR COMPLETENESS OF RESPONSES, OF RESULTS, OR CONDITIONS OF MERCHANTABILITY.
 * THE ENTIRE RISK OF THE USE OR THE RESULTS FROM THE USE OF THIS SAMPLE CODE REMAINS WITH THE USER.
 * NO TECHNICAL SUPPORT IS PROVIDED.  YOU MAY NOT DISTRIBUTE THIS CODE UNLESS YOU HAVE A LICENSE AGREEMENT WITH MICROSOFT THAT ALLOWS YOU TO DO SO.
 */

/* 
 IMPORTANT!!!
 THIS IS SAMPLE CODE ONLY.  
 THE CODE SHOULD BE UPDATED TO WORK WITH THE APPROPRIATE PAYMENT PROVIDERS.
 PROPER MESASURES SHOULD BE TAKEN TO ENSURE THAT THE PA-DSS AND PCI DSS REQUIREMENTS ARE MET. 
*/
namespace Contoso
{
    namespace Commerce.HardwareStation.Peripherals.PaymentTerminal.SampleMX925.Transport
    {
        using System;
        using System.Collections.Generic;
        using System.Diagnostics;
        using System.Net.Security;
        using System.Security.Authentication;
        using System.Security.Cryptography.X509Certificates;
        using System.Threading.Tasks;
        using Microsoft.Dynamics.Commerce.HardwareStation.Configuration;
    
        /// <summary>
        ///  Transport class to connect to <c>VeriFone</c> device over ethernet with SSL.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Tls", Justification = "Well known acronym.")]
        public class TcpTlsTransport : TcpTransport
        {
            private const string DefaultVerifoneSslHostName = "VFIMXSERVER";
            private const string DefaultVerifoneCertificateAuthority = "VFIMXCA";
            private const string VerifoneSslHostNameKey = "SSLHOSTNAME";
            private const string VerifoneCertificateAuthorityKey = "CLIENTCA";
    
            private readonly string verifoneDeviceSslHostName;
            private readonly string verifoneCertificateAuthority;
    
            /// <summary>
            ///  Initializes a new instance of the <see cref="TcpTlsTransport" /> class.
            /// </summary>
            /// <param name="config">Case insensitive configuration parameters.</param>
            public TcpTlsTransport(IDictionary<string, string> config)
                : base(config)
            {
                this.verifoneDeviceSslHostName = config.GetValueOrDefault(VerifoneSslHostNameKey, DefaultVerifoneSslHostName);
                this.verifoneCertificateAuthority = config.GetValueOrDefault(VerifoneCertificateAuthorityKey, DefaultVerifoneCertificateAuthority);
            }
    
            /// <summary>
            ///  ConnectAsync to the device.
            /// </summary>
            /// <returns>A task that closes the connection to the device.</returns>
            public override async Task ConnectAsync()
            {
                await base.ConnectAsync();
    
                var sslStream = new SslStream(this.Stream, false, (sender, cert, chain, sslPolicyError) => sslPolicyError == SslPolicyErrors.None, null);
                sslStream.AuthenticateAsClient(this.verifoneDeviceSslHostName, this.GetClientCerts(), SslProtocols.Ssl3, true);
                this.Stream = sslStream;
            }
    
            /// <summary>
            ///  Gets the certificates in the current user store which were issued by the <c>VeriFone</c> certificate authority.
            /// </summary>
            /// <returns>A collection of certificates.</returns>
            private X509CertificateCollection GetClientCerts()
            {
                var userStore = new X509Store(StoreLocation.CurrentUser);
                userStore.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
    
                var certs = new X509CertificateCollection();
                foreach (var certificate in userStore.Certificates)
                {
                    if (certificate.Issuer.Contains(this.verifoneCertificateAuthority))
                    {
                        certs.Add(certificate);
                        break;
                    }
                }
    
                return certs;
            }
        }
    }
}
