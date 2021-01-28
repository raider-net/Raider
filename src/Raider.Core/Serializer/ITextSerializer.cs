using System.Text;

namespace Raider.Serializer
{
	public interface ITextSerializer
	{
		void WriteTo(StringBuilder sb, string? before = null, string? after = null);
	}
}
