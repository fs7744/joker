using System.Security.Cryptography.X509Certificates;

namespace Joker.Protocols.Tls
{
    public interface ITlsConnectionFeature
    {
        X509Certificate2? ClientCertificate { get; set; }

        Task<X509Certificate2?> GetClientCertificateAsync(CancellationToken cancellationToken);
    }
}