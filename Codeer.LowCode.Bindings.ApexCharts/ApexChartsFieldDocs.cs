using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Codeer.LowCode.Bindings.ApexCharts.Designs;

namespace Codeer.LowCode.Bindings.ApexCharts
{
    /// <summary>
    /// ApexCharts 系フィールドの AI 用ドキュメント(Markdown)を返す。
    /// このライブラリはデザイナ(Codeer.LowCode.Blazor.Designer)を参照しないため、ここでは登録せず
    /// フィールドデザイン型 → Markdown の対応だけを提供する。
    /// 実際のデザイナへの登録は Designer 側(ApexChartsDesignerInitializer)が FieldCatalog.Add で行う。
    /// Markdown は本アセンブリの埋め込みリソース(FieldDocs/&lt;型名&gt;.md)から読み込む。
    /// </summary>
    public static class ApexChartsFieldDocs
    {
        static readonly Assembly Asm = typeof(ApexChartsFieldDocs).Assembly;

        public static Dictionary<Type, string> GetFieldDocs()
        {
            var result = new Dictionary<Type, string>();
            Add(result, typeof(ApexChartFieldDesign));
            Add(result, typeof(ApexHBarChartFieldDesign));
            Add(result, typeof(ApexRadialChartFieldDesign));
            return result;
        }

        static void Add(Dictionary<Type, string> result, Type fieldDesignType)
        {
            var md = LoadDoc(fieldDesignType.Name);
            if (md != null) result[fieldDesignType] = md;
        }

        static string? LoadDoc(string typeName)
        {
            var name = $"{Asm.GetName().Name}.FieldDocs.{typeName}.md";
            using var stream = Asm.GetManifestResourceStream(name);
            if (stream == null) return null;
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
