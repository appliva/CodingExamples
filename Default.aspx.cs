using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.IO;

using WebApplication.Models;
using WebApplication.Logic;

namespace WebApplication
{
    public partial class _Default : Page
    {
        private readonly AppContext _db = new WebApplication.Models.AppContext();
        private readonly CommonFunctions _cf = new CommonFunctions();

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        public class PseudoMultimediaItem : Multimedia
        {
            public int mediaId { get; set; }
        }

        public List<WebApplication.Models.Menu> GetMenu()
        {
            var query = (from s in _db.Menu
                         where s.MenuType.ID == 2 //center homepage menu
                         //&& s.active == true //active
                         orderby s.viewOrder ascending
                         select s).ToList();

            return query;
        }

        protected void rCenterMainMenu_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                int itemId = Convert.ToInt32((e.Item.FindControl("menuId") as Label).Text);
                int pageId = Convert.ToInt32((e.Item.FindControl("pageId") as Label).Text);
                string url = HttpContext.Current.Request.Url.AbsoluteUri;
                var savePath = HttpContext.Current.Server.MapPath("~/Uploads/Images"); //path        

                Literal liMenuTopLiteral = e.Item.FindControl("liMenuTopLiteral") as Literal;
                Literal liMenuBottomLiteral = e.Item.FindControl("liMenuBottomLiteral") as Literal;
                HyperLink menuHyperLink = e.Item.FindControl("menuHyperLink") as HyperLink;
                Image iconImage = e.Item.FindControl("iconImage") as Image;
                Literal descriptionLiteral = e.Item.FindControl("descriptionLiteral") as Literal;
                Image mainImage = e.Item.FindControl("mainImage") as Image;

                var query = (from s in _db.Pages
                             where s.ID == pageId
                             select new
                             {
                                 ID = s.ID,
                                 name = s.name,
                                 active = s.active,
                                 description = s.description
                             }).SingleOrDefault();

                if (query != null)
                {
                    if (query.active == true)
                    {
                        liMenuTopLiteral.Visible = true;
                        liMenuBottomLiteral.Visible = true;

                        if (query.name != "")
                        {
                            menuHyperLink.Visible = true;
                            menuHyperLink.Text = query.name;
                            menuHyperLink.NavigateUrl = Convert.ToString(GetRouteUrl("PagerRoute", new { page = _cf.ReplaceStringSpaces("spaces", query.name) }));
                        }
                        else
                            menuHyperLink.Visible = false;

                        if (query.description != "")
                        {
                            descriptionLiteral.Visible = true;
                            descriptionLiteral.Text = query.description;
                        }
                        else
                            descriptionLiteral.Visible = false;

                        //media
                        var mediaIcon = (from se in _db.Multimedia
                                         join ct in _db.MultimediaItems on se.ID equals ct.mediaId
                                         where ct.parentId == pageId
                                         where ct.locationId == 1 //icon
                                         select new PseudoMultimediaItem()
                                         {
                                             ID = se.ID,
                                             typeId = se.typeId,
                                             name = se.name,
                                             mediaItem = se.mediaItem,
                                             mediaId = ct.ID
                                         }).FirstOrDefault();

                        if (mediaIcon != null)
                        {
                            if (File.Exists(savePath + "/" + mediaIcon.mediaItem))
                            {
                                iconImage.Visible = true;
                                iconImage.ImageUrl = "~/Uploads/Images\\" + mediaIcon.mediaItem;
                            }
                            else
                                iconImage.Visible = false;
                        }
                        else
                            iconImage.Visible = false;

                        var mediaMain = (from se in _db.Multimedia
                                         join ct in _db.MultimediaItems on se.ID equals ct.mediaId
                                         where ct.parentId == pageId
                                         where ct.locationId == 2 //main image
                                         select new PseudoMultimediaItem()
                                         {
                                             ID = se.ID,
                                             typeId = se.typeId,
                                             name = se.name,
                                             mediaItem = se.mediaItem,
                                             mediaId = ct.ID
                                         }).FirstOrDefault();

                        if (mediaMain != null)
                        {
                            if (File.Exists(savePath + "/" + mediaMain.mediaItem))
                            {
                                mainImage.Visible = true;
                                mainImage.ImageUrl = "~/Uploads/Images\\" + mediaMain.mediaItem;
                            }
                            else
                                mainImage.Visible = false;
                        }
                        else
                            mainImage.Visible = false;
                        //end
                    }
                    else
                    {
                        liMenuTopLiteral.Visible = false;
                        liMenuBottomLiteral.Visible = false;
                        menuHyperLink.Visible = false;
                    }
                }

                //if (e.Item.ItemIndex == 0)
                //    menuHeaderHyperLink.CssClass = "first";
            }
        }

    }
}