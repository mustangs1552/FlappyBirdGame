#pragma checksum "D:\SourcetreeProjects\FlappyBirdGame\Website\PersonalProjectSite\PersonalProjectSite\Views\Game\Index.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "e117947d5b1e00e5242eb22787248f7e6e5ec9ec"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_Game_Index), @"mvc.1.0.view", @"/Views/Game/Index.cshtml")]
[assembly:global::Microsoft.AspNetCore.Mvc.Razor.Compilation.RazorViewAttribute(@"/Views/Game/Index.cshtml", typeof(AspNetCore.Views_Game_Index))]
namespace AspNetCore
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
#line 1 "D:\SourcetreeProjects\FlappyBirdGame\Website\PersonalProjectSite\PersonalProjectSite\Views\_ViewImports.cshtml"
using PersonalProjectSite;

#line default
#line hidden
#line 2 "D:\SourcetreeProjects\FlappyBirdGame\Website\PersonalProjectSite\PersonalProjectSite\Views\_ViewImports.cshtml"
using PersonalProjectSite.Models;

#line default
#line hidden
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"e117947d5b1e00e5242eb22787248f7e6e5ec9ec", @"/Views/Game/Index.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"f5e3f9c5079bf9a2ca77df575835cf3cbde06d4a", @"/Views/_ViewImports.cshtml")]
    public class Views_Game_Index : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<GamesModel>
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
#line 2 "D:\SourcetreeProjects\FlappyBirdGame\Website\PersonalProjectSite\PersonalProjectSite\Views\Game\Index.cshtml"
  
    ViewData["Title"] = Model.GameName;

#line default
#line hidden
            BeginContext(67, 6, true);
            WriteLiteral("\r\n<h1>");
            EndContext();
            BeginContext(74, 14, false);
#line 6 "D:\SourcetreeProjects\FlappyBirdGame\Website\PersonalProjectSite\PersonalProjectSite\Views\Game\Index.cshtml"
Write(Model.GameName);

#line default
#line hidden
            EndContext();
            BeginContext(88, 14, true);
            WriteLiteral("</h1>\r\n<iframe");
            EndContext();
            BeginWriteAttribute("src", " src=\"", 102, "\"", 122, 1);
#line 7 "D:\SourcetreeProjects\FlappyBirdGame\Website\PersonalProjectSite\PersonalProjectSite\Views\Game\Index.cshtml"
WriteAttributeValue("", 108, Model.GameSrc, 108, 14, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(123, 59, true);
            WriteLiteral(" style=\"width:960px;height:600px;border:0\"></iframe>\r\n<div>");
            EndContext();
            BeginContext(183, 21, false);
#line 8 "D:\SourcetreeProjects\FlappyBirdGame\Website\PersonalProjectSite\PersonalProjectSite\Views\Game\Index.cshtml"
Write(Model.GameDescription);

#line default
#line hidden
            EndContext();
            BeginContext(204, 41, true);
            WriteLiteral("</div>\r\n<div>\r\n    <h3>High Scores</h3>\r\n");
            EndContext();
#line 11 "D:\SourcetreeProjects\FlappyBirdGame\Website\PersonalProjectSite\PersonalProjectSite\Views\Game\Index.cshtml"
      
        if (Model.HighScores == null || Model.HighScores.Count == 0)
        {

#line default
#line hidden
            BeginContext(334, 58, true);
            WriteLiteral("            <div>No High Scores, be the first one!</div>\r\n");
            EndContext();
#line 15 "D:\SourcetreeProjects\FlappyBirdGame\Website\PersonalProjectSite\PersonalProjectSite\Views\Game\Index.cshtml"
        }
        else
        {
            foreach (HighScoresModel score in Model.HighScores)
            {

#line default
#line hidden
            BeginContext(508, 49, true);
            WriteLiteral("                <div>\r\n                    <span>");
            EndContext();
            BeginContext(558, 19, false);
#line 21 "D:\SourcetreeProjects\FlappyBirdGame\Website\PersonalProjectSite\PersonalProjectSite\Views\Game\Index.cshtml"
                     Write(score.ScoreUsername);

#line default
#line hidden
            EndContext();
            BeginContext(577, 35, true);
            WriteLiteral("</span>\r\n                    <span>");
            EndContext();
            BeginContext(613, 11, false);
#line 22 "D:\SourcetreeProjects\FlappyBirdGame\Website\PersonalProjectSite\PersonalProjectSite\Views\Game\Index.cshtml"
                     Write(score.Score);

#line default
#line hidden
            EndContext();
            BeginContext(624, 33, true);
            WriteLiteral("</span>\r\n                </div>\r\n");
            EndContext();
#line 24 "D:\SourcetreeProjects\FlappyBirdGame\Website\PersonalProjectSite\PersonalProjectSite\Views\Game\Index.cshtml"
            }
        }
    

#line default
#line hidden
            BeginContext(690, 6, true);
            WriteLiteral("</div>");
            EndContext();
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<GamesModel> Html { get; private set; }
    }
}
#pragma warning restore 1591