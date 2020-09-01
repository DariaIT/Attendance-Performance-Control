using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Attendance_Performance_Control.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Attendance_Performance_Control.Helpers
{
    public static class HtmlHelpers
    {
        public static string IsSelected(this IHtmlHelper html, string controller = null, string action = null, string cssClass = null)
        {
            if (String.IsNullOrEmpty(cssClass))
                cssClass = "active";

            string currentAction = (string)html.ViewContext.RouteData.Values["action"];
            string currentController = (string)html.ViewContext.RouteData.Values["controller"];

            if (String.IsNullOrEmpty(controller))
                controller = currentController;

            if (String.IsNullOrEmpty(action))
                action = currentAction;

            return controller == currentController && action == currentAction ?
                cssClass : String.Empty;
        }

        public static string PageClass(this IHtmlHelper htmlHelper)
        {
            string currentAction = (string)htmlHelper.ViewContext.RouteData.Values["action"];
            return currentAction;
        }

        //Helper to display Enum Data Annotation in View
        public static string GetDisplayName(this Enum enumValue)
        {
            return enumValue.GetType()?
 .GetMember(enumValue.ToString())?[0]?
 .GetCustomAttribute<DisplayAttribute>()?
 .Name;
        }

    }
}
