﻿//------------------------------------------------------------------------------
// <auto-generated>
//     このコードはツールによって生成されました。
//     ランタイム バージョン:4.0.30319.42000
//
//     このファイルへの変更は、以下の状況下で不正な動作の原因になったり、
//     コードが再生成されるときに損失したりします。
// </auto-generated>
//------------------------------------------------------------------------------

namespace cOCR.Properties {
    using System;
    
    
    /// <summary>
    ///   ローカライズされた文字列などを検索するための、厳密に型指定されたリソース クラスです。
    /// </summary>
    // このクラスは StronglyTypedResourceBuilder クラスが ResGen
    // または Visual Studio のようなツールを使用して自動生成されました。
    // メンバーを追加または削除するには、.ResX ファイルを編集して、/str オプションと共に
    // ResGen を実行し直すか、または VS プロジェクトをビルドし直します。
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   このクラスで使用されているキャッシュされた ResourceManager インスタンスを返します。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("cOCR.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   厳密に型指定されたこのリソース クラスを使用して、すべての検索リソースに対し、
        ///   現在のスレッドの CurrentUICulture プロパティをオーバーライドします。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   (アイコン) に類似した型 System.Drawing.Icon のローカライズされたリソースを検索します。
        /// </summary>
        internal static System.Drawing.Icon cOCRIcon {
            get {
                object obj = ResourceManager.GetObject("cOCRIcon", resourceCulture);
                return ((System.Drawing.Icon)(obj));
            }
        }
        
        /// <summary>
        ///   div#description { 
        ///    margin: 2em;
        ///}
        ///
        ///div#image,
        ///div#image img,
        ///div#overlay img {
        ///    width: calc(100vw - 2em);
        ///    user-drag: none; 
        ///    user-select: none;
        ///    -moz-user-select: none;
        ///    -webkit-user-drag: none;
        ///    -webkit-user-select: none;
        ///    -ms-user-select: none;
        ///}
        ///div#overlay {
        ///    position: absolute;
        ///    opacity: 0;
        ///}
        ///div#overlay:hover {
        ///    opacity: 1;
        ///}
        ///div#overlay .block,
        ///div#overlay .paragraph,
        ///div#overlay .word {
        ///    position: absolute;
        ///    float: left;
        ///}
        ///div#over [残りの文字列は切り詰められました]&quot;; に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string DefaultCSS {
            get {
                return ResourceManager.GetString("DefaultCSS", resourceCulture);
            }
        }
        
        /// <summary>
        ///   
        ///function appendDescriptionText(jsonNode, text) {
        ///    var description = document.createElement(&quot;div&quot;);
        ///    description.id = &quot;description&quot;;
        ///    var node = document.createElement(&quot;span&quot;);
        ///    // Bacause `responses[0].textAnnotations[0].description` contains break lines as `\n`,
        ///    // it should be replaced by `&lt;br&gt;` here for using as a html string.
        ///    node.innerHTML = text.replace(/\n/g, &quot;&lt;br&gt;&quot;);
        ///    description.appendChild(node);
        ///    jsonNode.parentNode.insertBefore(description, jsonNode);
        ///}
        ///
        ///fu [残りの文字列は切り詰められました]&quot;; に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string DefaultJS {
            get {
                return ResourceManager.GetString("DefaultJS", resourceCulture);
            }
        }
    }
}
