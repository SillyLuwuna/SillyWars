using System.IO;
using System.IO.Compression;

namespace RtsEngine.Data
{

public static class DataCompressor
{
	public static byte[] CompressData(byte[] data)
	{
		using MemoryStream outputStream = new MemoryStream();
		using (BrotliStream zip = new BrotliStream(outputStream, CompressionLevel.Optimal))
		{
			zip.Write(data, 0, data.Length);
		}

		return outputStream.ToArray();
	}

	public static byte[] DecompressData(byte[] data)
	{
		using MemoryStream inputStream = new MemoryStream(data);
		using BrotliStream zip = new BrotliStream(inputStream, CompressionMode.Decompress);
		using MemoryStream outputStream = new MemoryStream();

		zip.CopyTo(outputStream);
		return outputStream.ToArray();
	}
}

}
