namespace Joker.Protocols.Tls
{
    public interface ITlsApplicationProtocolFeature
    {
        /// <summary>
        /// Gets the <see cref="ReadOnlyMemory{T}"/> represeting the application protocol.
        /// </summary>
        ReadOnlyMemory<byte> ApplicationProtocol { get; }
    }
}