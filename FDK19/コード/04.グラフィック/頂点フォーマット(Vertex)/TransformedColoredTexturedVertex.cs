using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Globalization;
using SharpDX;
using SharpDX.Direct3D9;

namespace FDK
{
	[StructLayout( LayoutKind.Sequential )]
	public struct TransformedColoredTexturedVertex : IEquatable<TransformedColoredTexturedVertex>
	{
		public Vector4	Position;
		public int		Color;
		public Vector2	TextureCoordinates;

		public static VertexFormat Format
		{
			get
			{
				return ( VertexFormat.Texture1 | VertexFormat.Diffuse | VertexFormat.PositionRhw );
			}
		}

		public static bool operator ==( TransformedColoredTexturedVertex left, TransformedColoredTexturedVertex right )
		{
			return left.Equals( right );
		}
		public static bool operator !=( TransformedColoredTexturedVertex left, TransformedColoredTexturedVertex right )
		{
			return !( left == right );
		}
		public override int GetHashCode()
		{
			return ( ( this.Position.GetHashCode() + this.Color.GetHashCode() ) + this.TextureCoordinates.GetHashCode() );
		}
		public override bool Equals( object obj )
		{
			if( obj == null )
			{
				return false;
			}
			if( base.GetType() != obj.GetType() )
			{
				return false;
			}
			return this.Equals( (TransformedColoredTexturedVertex) obj );
		}
		public bool Equals( TransformedColoredTexturedVertex other )
		{
			return ( ( ( this.Position == other.Position ) && ( this.Color == other.Color ) ) && ( this.TextureCoordinates == other.TextureCoordinates ) );
		}
		public override string ToString()
		{
			return string.Format( CultureInfo.CurrentCulture, "{0} ({1}, {2})", new object[] { this.Position.ToString(), System.Drawing.Color.FromArgb( this.Color ).ToString(), this.TextureCoordinates.ToString() } );
		}
	}
}
