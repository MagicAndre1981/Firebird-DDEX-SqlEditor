// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.Interfaces.IBinderQueue
using System;



namespace BlackbirdSql.LanguageExtension.Interfaces;


public interface IBsBinderQueue
{
	IAsyncResult EnqueueUIThreadAction(Func<object> f);

	IAsyncResult EnqueueBindAction(Func<object> f);

	IAsyncResult EnqueueRecomputeMetadataAction(Func<object> f);
}
