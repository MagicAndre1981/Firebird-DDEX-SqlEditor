﻿using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;



namespace BlackbirdSql.Sys.Controls.Widgets;


public class WrappingCheckBox : CheckBox
{
	private Size cachedSizeOfOneLineOfText = Size.Empty;

	private readonly Hashtable preferredSizeHash = new Hashtable(3);

	public WrappingCheckBox()
	{
		AutoSize = true;
	}

	protected override void OnTextChanged(EventArgs e)
	{
		base.OnTextChanged(e);
		Cachetextsize();
	}

	protected override void OnFontChanged(EventArgs e)
	{
		base.OnFontChanged(e);
		Cachetextsize();
	}

	private void Cachetextsize()
	{
		preferredSizeHash.Clear();
		if (string.IsNullOrEmpty(Text))
		{
			cachedSizeOfOneLineOfText = Size.Empty;
		}
		else
		{
			cachedSizeOfOneLineOfText = TextRenderer.MeasureText(Text, Font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.WordBreak);
		}
	}

	public override Size GetPreferredSize(Size proposedSize)
	{
		Size size = base.GetPreferredSize(proposedSize);
		if (size.Width > proposedSize.Width && (!string.IsNullOrEmpty(Text) && !proposedSize.Width.Equals(int.MaxValue) || !proposedSize.Height.Equals(int.MaxValue)))
		{
			Size size2 = size - cachedSizeOfOneLineOfText;
			Size size3 = proposedSize - size2 - new Size(3, 0);
			if (!preferredSizeHash.ContainsKey(size3))
			{
				size = size2 + TextRenderer.MeasureText(Text, Font, size3, TextFormatFlags.WordBreak);
				preferredSizeHash[size3] = size;
			}
			else
			{
				size = (Size)preferredSizeHash[size3];
			}
		}

		return size;
	}
}
