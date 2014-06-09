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
using AjaxControlToolkit;

using System.Data.SqlClient;
using System.Web.Services;
using WebApplication.Logic;

using WebApplication.Models;
using System.Web.ModelBinding;

namespace WebApplication
{
    public partial class _LogisticsInfrastructure : System.Web.UI.Page
    {
        private readonly AppContext _db = new WebApplication.Models.AppContext();
        private readonly CommonFunctions _cf = new CommonFunctions();
        private readonly SystemActions _sa = new SystemActions();
        private bool showMessageBox = false;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var action = Convert.ToString(Page.RouteData.Values["action"]);
                int id = Convert.ToInt32(Page.RouteData.Values["id"]);

                HttpCookie myCookie = Request.Cookies["pageViewCookie"];

                if (myCookie != null) //read cookie information
                {
                    if (myCookie.Value != "")
                    {
                        if (myCookie.Value == "Map")
                        {
                            mapMapViewPanel.Visible = true;
                            //mapListViewPanel.Visible = false;

                            logisticsInfrastructureMapPanel.Visible = true;
                            logisticsInfrastructureListPanel.Visible = false;

                            changeViewLinkButton.Text = "תצוגת רשימה";
                            changeViewLinkButton.CommandName = "List";

                            GetLogisticsMap(false).ToList();
                        }
                        else if (myCookie.Value == "List")
                        {
                            mapMapViewPanel.Visible = false;
                            //mapListViewPanel.Visible = true;

                            logisticsInfrastructureMapPanel.Visible = false;
                            logisticsInfrastructureListPanel.Visible = true;

                            changeViewLinkButton.Text = "תצוגת מפה";
                            changeViewLinkButton.CommandName = "Map";

                            gvLogisticsInfrastructure.DataSource = GetLogisticsInfrastructure(false).ToList();
                            gvLogisticsInfrastructure.DataBind();
                            gvLogisticsInfrastructure.UseAccessibleHeader = true;
                        }
                    }
                    else
                    {
                        mapMapViewPanel.Visible = true;
                        //mapListViewPanel.Visible = false;
                        logisticsInfrastructureMapPanel.Visible = true;
                        logisticsInfrastructureListPanel.Visible = false;

                        GetLogisticsMap(false).ToList();
                    }
                }
                else
                {
                    mapMapViewPanel.Visible = true;
                    //mapListViewPanel.Visible = false;
                    logisticsInfrastructureMapPanel.Visible = true;
                    logisticsInfrastructureListPanel.Visible = false;

                    GetLogisticsMap(false).ToList();

                    HttpCookie pageView = new HttpCookie("pageViewCookie");
                    DateTime now = DateTime.Now;

                    pageView.Value = "Map"; //set the cookie value.
                    pageView.Expires = now.AddHours(1);  //set the cookie expiration date.
                    Response.Cookies.Add(pageView); //add the cookie.
                }

                List<int> citiesList = new List<int>();
                string GetUserCities = _sa.UserCities();
                int citiesCount = 0;

                if (GetUserCities != null)
                {
                    if (GetUserCities != "")
                    {
                        string[] arr = GetUserCities.Split(',');

                        foreach (string i in arr)
                        {
                            citiesList.Add(Convert.ToInt32(i));
                        }

                        citiesCount = citiesList.Count;
                    }
                }

                if (citiesCount == 1)
                {
                    cityIdFilter.SelectedValue = Convert.ToString(citiesList[0]);
                    cityIdFilter.Enabled = false;

                    streetIdFilter.DataSource = GetStreets("View", citiesList[0]).ToList();
                    streetIdFilter.DataBind();
                }
                else
                {
                    streetIdFilter.DataSource = GetStreets("View", 0).ToList();
                    streetIdFilter.DataBind();
                }

                if (action == "Edit" && id != 0)
                {
                    GetLogistics("Edit", id);
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowModalEditAction", "$(function() { ShowModal('#myModalEditItem'); });", true);
                }
                else if (action == "View" && id == 0)
                {

                }
                else if (action == "Insert")
                {

                }
                else if (action == "Add" && id == 0)
                {
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowModalInsertAction", "$(function() { ShowModal('#myModalEditPopulation'); });", true);
                }

            }
        }

        public class PseudoItems : LogisticsInfrastructure
        {
            public int locationId { get; set; }
            public string type { get; set; }
            public string address { get; set; }
        }

        public IQueryable<LogisticsInfrastructure> GetLogisticsInfrastructure(bool? filter = false)
        {
            var action = Convert.ToString(Page.RouteData.Values["action"]);
            int id = Convert.ToInt32(Page.RouteData.Values["id"]);
            string pageTitleString = "";

            int GetUserClient = _sa.UserClient(); //get user admin client id

            //filter admin city
            List<int> citiesList = new List<int>();
            string GetUserCities = _sa.UserCities();

            if (GetUserCities != null)
            {
                if (GetUserCities != "")
                {
                    string[] arr = GetUserCities.Split(',');

                    foreach (string i in arr)
                    {
                        citiesList.Add(Convert.ToInt32(i));
                    }
                }
            }
            //end

            var query = _db.LogisticsInfrastructures.AsQueryable();

            query = from p in _db.LogisticsInfrastructures
                    join cb in _db.LogisticsInfrastructureTypes on p.typeId equals cb.ID //typeId name
                    join cbb in _db.LogisticsInfrastructureTypes on p.subTypeId equals cbb.ID //subTypeId name
                    where p.client.ID == GetUserClient
                    && p.statusId != 2 //0 = not active, 1 = active, 2 = deleted
                    && citiesList.Contains(p.location.cityKey.SignId)
                    orderby p.ID descending
                    select new PseudoItems()
                    {
                        ID = p.ID,
                        name = p.name,
                        typeId = p.typeId,
                        subTypeId = p.subTypeId,
                        //location
                        cityId =  (p.location.cityKey.SignId != null ? p.location.cityKey.SignId : 0),
                        streetId = (p.location.streetKey.ID != null ? p.location.streetKey.ID : 0),
                        //cityId = 0,
                        //streetId = 0,
                        Street = p.location.Street,
                        streetNumber = p.location.streetNumber,
                        apartmentNumber = p.location.apartmentNumber,
                        //end

                        type = cb.name + ", " + cbb.name,
                        address = (p.location.streetKey.Name != null ? (p.location.streetKey.Name + (p.location.streetNumber != "" ? " " + p.location.streetNumber : "") + (p.location.apartmentNumber != "" ? " (" + p.location.apartmentNumber + ")" : "") + ", ") : "") + (p.location.cityKey.Name != null ? p.location.cityKey.Name : "")
                        //address = "e"
                    };

            if (filter == true)
            {
                pageTitle.Text = "תשתיות - תוצאות חיפוש פילוח";

                int typeIdCheck = Convert.ToInt32(filterTypeId.SelectedValue);
                int subTypeIdCheck = 0;

                if (filterSubTypeId.Items.FindByValue("0") != null)
                    subTypeIdCheck = Convert.ToInt32(filterSubTypeId.SelectedValue);

                string nameCheck = nameFilter.Text;
                int cityIdCheck = Convert.ToInt32(cityIdFilter.SelectedValue);
                int streetIdCheck = Convert.ToInt32(streetIdFilter.SelectedValue);
                string streetCheck = Convert.ToString(streetFilter.Text);
                string streetNumberCheck = Convert.ToString(streetNumberFilter.Text);
                string apartmentNumberCheck = Convert.ToString(apartmentNumberFilter.Text);

                if (streetCheck != null)
                    streetCheck = streetCheck.Replace('+', ' ');

                //add the filter parameters
                if (typeIdCheck != 0)
                    query = query.Where(c => c.typeId == typeIdCheck);

                if (subTypeIdCheck != 0)
                    query = query.Where(c => c.subTypeId == subTypeIdCheck);

                if (nameCheck != "")
                    query = query.Where(c => c.name == nameCheck);

                if (cityIdCheck != 0)
                    query = query.Where(c => c.cityId == cityIdCheck);

                if (streetIdCheck != 0)
                    query = query.Where(c => c.streetId == streetIdCheck);

                if (streetCheck != "")
                    query = query.Where(c => c.Street == streetCheck);

                if (streetNumberCheck != "")
                    query = query.Where(c => c.streetNumber == streetNumberCheck);

                if (apartmentNumberCheck != "")
                    query = query.Where(c => c.apartmentNumber == apartmentNumberCheck);
                //end
            }

            if (action != "Edit")
            {
                if (id != 0)
                {
                    query = query.Where(c => c.typeId == id);

                    var query2 = (from p in _db.LogisticsInfrastructureTypes
                                  where p.ID == id
                                  select new
                                  {
                                      name = p.name
                                  }).SingleOrDefault();

                    pageTitleString = "תשתיות - " + query2.name;
                }
                else
                {
                    pageTitleString = "תשתיות - תצוגת רשימה";
                }
            }

            pageTitle.Text = pageTitleString;
            tableTitle.Text = pageTitleString;

            //finally select the columns we needed
            var result =
            from p in query
            orderby p.ID descending
            select p;

            return result;
        }

        protected void gvLogisticsInfrastructure_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {

            }
        }

        protected void gvLogisticsInfrastructure_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvLogisticsInfrastructure.PageIndex = e.NewPageIndex;
            gvLogisticsInfrastructure.DataSource = GetLogisticsInfrastructure(false).ToList();
            gvLogisticsInfrastructure.DataBind();
        }

        public IQueryable<LogisticsInfrastructureType> GetLogisticsInfrastructureType()
        {
            var quary = _db.LogisticsInfrastructureTypes.AsQueryable();

            quary = (from s in _db.LogisticsInfrastructureTypes
                     where s.parentID == 0
                     //&& s.status == 1 //active
                     select s);

            return quary;
        }

        protected void typeId_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedTypeId = Convert.ToInt32(typeId.SelectedValue);

            if (selectedTypeId != 0)
            {
                string strConn = ConfigurationManager.ConnectionStrings["WebApplicationConnectionString"].ConnectionString;

                using (SqlConnection con = new SqlConnection(strConn))
                {
                    con.Open();

                    using (SqlCommand cmd = new SqlCommand("select * from LogisticsInfrastructureTypes where parentID = @parentID", con))
                    {
                        cmd.Parameters.AddWithValue("@parentID", selectedTypeId);
                        SqlDataReader dr = cmd.ExecuteReader();

                        subTypeId.DataTextField = "name";
                        subTypeId.DataValueField = "ID";
                        subTypeId.DataSource = dr;
                        subTypeId.DataBind();

                        subTypeId.Items.Insert(0, new ListItem("--- בחר ---", "0"));
                    }
                }

                subTypeIdPanel.Visible = true;
            }
            else
            {
                subTypeIdPanel.Visible = false;
            }
        }

        protected void typeId_DataBound(object sender, EventArgs e)
        {
            typeId.Items.Insert(0, new ListItem("--- בחר ---", "0"));
        }

        protected void subTypeId_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedSubTypeId = Convert.ToInt32(subTypeId.SelectedValue);
            int checkItemId = Convert.ToInt32(itemId.Text);

            if (selectedSubTypeId != 0)
            {
                var item = _db.LogisticsInfrastructures.Find(checkItemId);

                if (checkItemId != 0)
                {
                    if (item.subTypeId == selectedSubTypeId)
                        gvEquipment.DataSource = GetEquipment(checkItemId).ToList();
                    else
                        gvEquipment.DataSource = GetEquipmentTypes(selectedSubTypeId).ToList();

                    saveAllEquipmentButton.Visible = true;
                }
                else
                {
                    gvEquipment.DataSource = GetEquipmentTypes(selectedSubTypeId).ToList();
                    saveAllEquipmentButton.Visible = false;
                }

                gvEquipment.DataBind();
                equipmentUpdatePanel.Update();
            }
            else
            {
                /*(if (checkItemId != 0)
                {
                    gvEquipment.DataSource = GetEquipment(checkItemId).ToList();
                    saveAllEquipmentButton.Visible = true;
                }
                else
                {
                    gvEquipment.DataSource = GetEquipmentTypes(selectedSubTypeId).ToList();
                    saveAllEquipmentButton.Visible = false;
                }
                */

                gvEquipment.DataSource = GetEquipmentTypes(0).ToList();
                gvEquipment.DataBind();

                //equipmentPanel.Visible = false;
                equipmentUpdatePanel.Update();
            }
        }

        protected void filterTypeId_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedTypeId = Convert.ToInt32(filterTypeId.SelectedValue);

            if (selectedTypeId != 0)
            {
                string strConn = ConfigurationManager.ConnectionStrings["WebApplicationConnectionString"].ConnectionString;

                using (SqlConnection con = new SqlConnection(strConn))
                {
                    con.Open();

                    using (SqlCommand cmd = new SqlCommand("select * from LogisticsInfrastructureTypes where parentID = @parentID", con))
                    {
                        cmd.Parameters.AddWithValue("@parentID", selectedTypeId);
                        SqlDataReader dr = cmd.ExecuteReader();

                        filterSubTypeId.DataTextField = "name";
                        filterSubTypeId.DataValueField = "ID";
                        filterSubTypeId.DataSource = dr;
                        filterSubTypeId.DataBind();

                        filterSubTypeId.Items.Insert(0, new ListItem("--- בחר ---", "0"));
                    }
                }

                filterSubTypeIdPanel.Visible = true;
            }
            else
            {
                filterSubTypeIdPanel.Visible = false;
            }
        }

        protected void filterTypeId_DataBound(object sender, EventArgs e)
        {
            filterTypeId.Items.Insert(0, new ListItem("--- בחר ---", "0"));
        }

        public IQueryable<City> GetCities()
        {
            //ddlStreetId.DataSource = GetStreets("View", 0).ToList();
            //ddlStreetId.DataBind();

            //streetIdFilter.DataSource = GetStreets("View", 0).ToList();
            //streetIdFilter.DataBind(); 

            List<int> citiesList = new List<int>();

            SystemActions _sa = new SystemActions();
            string GetUserCities = _sa.UserCities();

            if (GetUserCities != null)
            {
                if (GetUserCities != "")
                {
                    string[] arr = GetUserCities.Split(',');

                    foreach (string i in arr)
                    {
                        citiesList.Add(Convert.ToInt32(i));
                    }
                }
            }

            var query = _db.Cities.AsQueryable();

            query = from c in _db.Cities
                    where c.Active == true
                    && citiesList.Contains(c.SignId)
                    orderby c.ID ascending
                    select c;

            var result = from c in query
                         select c;

            return result;
        }

        protected void cityId_DataBound(object sender, EventArgs e)
        {
            cityIdFilter.Items.Insert(0, new ListItem("--- בחר ---", "0"));
        }

        public IQueryable<Street> GetStreets(string action, int? id)
        {
            var query = _db.Streets.AsQueryable();

            query = from c in _db.Streets
                    where (c.StatusId == 1 || c.StatusId == 0) //active
                    && c.CityId == id
                    orderby c.ID ascending
                    select c;

            return query;
        }

        protected void ddlStreetId_DataBound(object sender, EventArgs e)
        {
            ddlStreetId.Items.Insert(0, new ListItem("--- בחר ---", "0"));
        }

        public IQueryable<Population> GetLogisticsInfrastructurePopulation()
        {
            List<int> citiesList = new List<int>();

            SystemActions _sa = new SystemActions();
            string GetUserCities = _sa.UserCities();

            if (GetUserCities != null)
            {
                if (GetUserCities != "")
                {
                    string[] arr = GetUserCities.Split(',');

                    foreach (string i in arr)
                    {
                        citiesList.Add(Convert.ToInt32(i));
                    }
                }
            }

            var query = _db.Populations.AsQueryable();

            query = from c in _db.Populations
                    where c.status == 1 //active
                    && citiesList.Contains(c.location.cityKey.SignId)
                    orderby c.ID ascending
                    select c;

            var result = from c in query
                         select c;

            return result;
        }

        public IQueryable<TeamLevelType> GetTeamLevelTypes()
        {
            var query = _db.TeamsLevelTypes.AsQueryable();

            query = (from s in _db.TeamsLevelTypes
                     where s.status == 1 //active
                     && s.typeId == 1 //מחלקות צוות חירום יישובי
                     orderby s.orderId ascending
                     select s);

            return query;
        }

        protected void submitFilterButton_Click(object sender, EventArgs e)
        {
            HttpCookie myCookie = Request.Cookies["pageViewCookie"];

            if (myCookie != null) //read cookie information
            {
                if (myCookie.Value != "")
                {
                    //Response.Write("#" + myCookie.Value);

                    if (myCookie.Value == "Map")
                    {
                        GetLogisticsMap(true);

                        mapMapViewPanel.Visible = true;
                        //mapListViewPanel.Visible = false;

                        logisticsInfrastructureMapPanel.Visible = true;
                        logisticsInfrastructureListPanel.Visible = false;

                        changeViewLinkButton.Text = "תצוגת רשימה";
                        changeViewLinkButton.CommandName = "List";
                    }
                    else if (myCookie.Value == "List")
                    {
                        gvLogisticsInfrastructure.DataSource = GetLogisticsInfrastructure(true).ToList();
                        gvLogisticsInfrastructure.DataBind();
                        gvLogisticsInfrastructure.UseAccessibleHeader = true;

                        mapMapViewPanel.Visible = false;
                        //mapListViewPanel.Visible = true;

                        logisticsInfrastructureMapPanel.Visible = false;
                        logisticsInfrastructureListPanel.Visible = true;

                        changeViewLinkButton.Text = "תצוגת מפה";
                        changeViewLinkButton.CommandName = "Map";
                    }
                }
                else
                {
                    GetLogisticsMap(true);

                    mapMapViewPanel.Visible = true;
                    //mapListViewPanel.Visible = false;
                    logisticsInfrastructureMapPanel.Visible = true;
                    logisticsInfrastructureListPanel.Visible = false;
                }
            }
            else
            {
                GetLogisticsMap(true);

                mapMapViewPanel.Visible = true;
                //mapListViewPanel.Visible = false;
                logisticsInfrastructureMapPanel.Visible = true;
                logisticsInfrastructureListPanel.Visible = false;
            }
        }

        protected void clearFilterButton_Click(object sender, EventArgs e)
        {
            //clear filter form
            filterTypeId.SelectedValue = Convert.ToString("0");
            filterSubTypeId.SelectedValue = Convert.ToString("0");
            filterSubTypeIdPanel.Visible = false;
            nameFilter.Text = "";
            cityIdFilter.SelectedValue = Convert.ToString(0);
            streetFilter.Text = "";
            streetNumberFilter.Text = "";
            apartmentNumberFilter.Text = "";
            //end

            HttpCookie myCookie = Request.Cookies["pageViewCookie"];

            if (myCookie != null) //read cookie information
            {
                if (myCookie.Value != "")
                {
                    if (myCookie.Value == "Map")
                    {
                        GetLogisticsMap(false).ToList();

                        mapMapViewPanel.Visible = true;
                        //mapListViewPanel.Visible = false;

                        logisticsInfrastructureMapPanel.Visible = true;
                        logisticsInfrastructureListPanel.Visible = false;

                        changeViewLinkButton.Text = "תצוגת רשימה";
                        changeViewLinkButton.CommandName = "List";
                    }
                    else if (myCookie.Value == "List")
                    {
                        gvLogisticsInfrastructure.DataSource = GetLogisticsInfrastructure(false).ToList();
                        gvLogisticsInfrastructure.DataBind();
                        gvLogisticsInfrastructure.UseAccessibleHeader = true;

                        mapMapViewPanel.Visible = false;
                        //mapListViewPanel.Visible = true;

                        logisticsInfrastructureMapPanel.Visible = false;
                        logisticsInfrastructureListPanel.Visible = true;

                        changeViewLinkButton.Text = "תצוגת מפה";
                        changeViewLinkButton.CommandName = "Map";
                    }
                }
                else
                {
                    GetLogisticsMap(false).ToList();

                    mapMapViewPanel.Visible = true;
                    //mapListViewPanel.Visible = false;
                    logisticsInfrastructureMapPanel.Visible = true;
                    logisticsInfrastructureListPanel.Visible = false;
                }
            }
            else
            {
                GetLogisticsMap(false).ToList();

                mapMapViewPanel.Visible = true;
                //mapListViewPanel.Visible = false;
                logisticsInfrastructureMapPanel.Visible = true;
                logisticsInfrastructureListPanel.Visible = false;
            }
        }

        protected void GetLogisticsInfrastructure_Click(object sender, CommandEventArgs e)
        {
            int id = Convert.ToInt32(e.CommandArgument);
            string action = Convert.ToString(e.CommandName);

            if (action == "InsertItem")
            {
                GetLogistics("Insert", id);
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowModalInsertAction", "$(function() { ShowModal('#myModalEditItem'); });", true);
            }
            else if (action == "EditItem")
            {
                GetLogistics("Edit", id);
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowModalEditAction", "$(function() { ShowModal('#myModalEditItem'); });", true);
            }
        }

        protected void ddlCityId_DataBound(object sender, EventArgs e)
        {
            ddlCityId.Items.Insert(0, new ListItem("--- בחר ---", "0"));
        }

        protected void ddlCityId_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedCityId = Convert.ToInt32(ddlCityId.SelectedValue);

            if (selectedCityId != 0)
            {
                Label suggestedCityId = (Label)this.Master.FindControl("suggestedCityId");
                suggestedCityId.Text = Convert.ToString(selectedCityId);

                ddlStreetId.DataSource = GetStreets("View", selectedCityId).ToList();
                ddlStreetId.DataBind();

                //get map
                string latLng = "";

                var itemCities = (from p in _db.Cities
                                  where p.SignId == selectedCityId
                                  select p).SingleOrDefault();

                latLng = itemCities.Lat + "," + itemCities.Lng;

                string marker = callMap("city", 2, latLng);
                ScriptManager.RegisterStartupScript(this, this.GetType(), "CallMarkerAction", marker, true);
                //end

                street.Text = "";
                streetNumber.Text = "";
                apartmentNumber.Text = "";
            }
        }

        protected void ddlStreetId_SelectedIndexChanged(object sender, EventArgs e)
        {
            street.Text = "";
            streetNumber.Text = "";
            apartmentNumber.Text = "";
        }

        protected void cityIdFilter_DataBound(object sender, EventArgs e)
        {
            cityIdFilter.Items.Insert(0, new ListItem("--- הכל ---", "0"));
        }

        protected void cityIdFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedCityId = Convert.ToInt32(cityIdFilter.SelectedValue);

            if (selectedCityId != 0)
            {
                streetIdFilter.DataSource = GetStreets("View", selectedCityId).ToList();
                streetIdFilter.DataBind();
            }
        }

        protected void streetIdFilter_DataBound(object sender, EventArgs e)
        {
            streetIdFilter.Items.Insert(0, new ListItem("--- הכל ---", "0"));
        }

        public class PseudoLogisticsItem : LogisticsInfrastructure
        {
            public int cityId { get; set; }
            public int streetId { get; set; }
            public int locationId { get; set; }
            public string streetNumber { get; set; }
            public string apartmentNumber { get; set; }

            public string city { get; set; }
            public string street { get; set; }
            public string polygonDetails { get; set; }
            public string markerDetails { get; set; }
            public string address { get; set; }
            public string latitude { get; set; }
        }

        public LogisticsInfrastructure GetLogistics(string action, int? id)
        {
            List<int> citiesList = new List<int>();
            string GetUserCities = _sa.UserCities();
            int citiesCount = 0;

            if (GetUserCities != null)
            {
                if (GetUserCities != "")
                {
                    string[] arr = GetUserCities.Split(',');

                    foreach (string i in arr)
                    {
                        citiesList.Add(Convert.ToInt32(i));
                    }

                    citiesCount = citiesList.Count;
                }
            }

            var item = (from se in _db.LogisticsInfrastructures
                        where se.ID == id
                        select new PseudoLogisticsItem()
                        {
                            ID = se.ID,
                            name = se.name,
                            typeId = se.typeId,
                            subTypeId = se.subTypeId,
                            //location
                            cityId = (se.location.cityKey.SignId == null) ? 0 : se.location.cityKey.SignId,
                            streetId = (se.location.streetKey.ID == null) ? 0 : se.location.streetKey.ID,
                            city = se.location.City,
                            street = se.location.Street,
                            streetNumber = se.location.streetNumber,
                            apartmentNumber = se.location.apartmentNumber,
                            polygonDetails = se.location.mapArea,
                            markerDetails = se.location.latitude,
                            latitude = se.location.latitude,
                            locationId = (se.location == null) ? 0 : se.location.ID
                            //end
                        }).FirstOrDefault();

            Panel subTypeIdPanel = this.Master.FindControl("MainContent").FindControl("subTypeIdPanel") as Panel;

            infrastructureId.Text = Convert.ToString(id);
            showMessageBox = ShowMessageBoxAlert(false, "", "");

            addInfrastructureAlertPanel.Visible = false;
            addInfrastructureAlertPanel.CssClass = "";
            addInfrastructureAlertLabel.Text = "";

            HttpCookie myCookie = Request.Cookies["pageViewCookie"];

            if (myCookie != null) //read cookie information
            {
                //Response.Redirect("/x=" + myCookie.Value);

                if (myCookie.Value != "")
                {
                    if (myCookie.Value == "Map")
                    {
                        mapMapViewPanel.Visible = false;
                        //mapListViewPanel.Visible = true;

                        logisticsInfrastructureMapPanel.Visible = false;
                        logisticsInfrastructureListPanel.Visible = true;

                        changeViewLinkButton.Text = "תצוגת מפה";
                        changeViewLinkButton.CommandName = "Map";

                        gvLogisticsInfrastructure.DataSource = GetLogisticsInfrastructure(false).ToList();
                        gvLogisticsInfrastructure.DataBind();
                        gvLogisticsInfrastructure.UseAccessibleHeader = true;

                        HttpCookie pageView = new HttpCookie("pageViewCookie");
                        DateTime now = DateTime.Now;

                        pageView.Value = "List"; //set the cookie value.
                        pageView.Expires = now.AddHours(1);  //set the cookie expiration date.
                        Response.Cookies.Add(pageView); //add the cookie.
                    }
                }
            }

            js.Text = "";

            ////mapListViewPanel.Visible = true;
            ////mapMapViewPanel.Visible = false;

            if (item != null)
            {
                if (action == "View")
                {

                }
                else if (action == "Edit")
                {
                    updateItemTitleModal.Text = "עריכת תשתית - " + item.name;

                    saveButton.CommandName = "Edit";
                    saveButton.Text = "עדכן פרטי תשתית";

                    deleteButton.Visible = true;

                    itemId.Text = Convert.ToString(id);
                    typeId.SelectedValue = Convert.ToString(item.typeId);
                    name.Text = item.name;
                    string latLng = "";

                    //location
                    if (item.locationId != 0)
                    {
                        if (item.cityId != 0)
                        {
                            if (citiesCount == 1)
                            {
                                //ddlCityId.Enabled = false;
                                //cityPanel.Style.Add("display", "none");  
                            }

                            ddlCityId.SelectedValue = Convert.ToString(item.cityId);

                            ddlStreetId.DataSource = GetStreets("View", item.cityId).ToList();
                            ddlStreetId.DataBind();
                            ddlStreetId.SelectedValue = Convert.ToString(item.streetId);

                            //Response.Redirect("/x=" + item.cityId + "-" + item.streetId);

                            city.Text = item.city;
                            street.Text = item.street;
                            streetNumber.Text = item.streetNumber;
                            apartmentNumber.Text = item.apartmentNumber;

                            polygonDetails.Text = item.mapArea;
                            markerDetails.Text = item.latitude;
                            latLng = item.latitude;
                        }
                    }
                    //end

                    //var query = _db.LogisticsInfrastructureTypes.AsQueryable();

                    var query = from c in _db.LogisticsInfrastructureTypes
                            where c.parentID == item.typeId
                            orderby c.ID ascending
                            select c;

                    subTypeId.DataTextField = "name";
                    subTypeId.DataValueField = "ID";
                    subTypeId.DataSource = query.ToList();
                    subTypeId.DataBind();

                    if (item.subTypeId > 0)
                        subTypeId.SelectedValue = Convert.ToString(item.subTypeId);

                    subTypeId.Items.Insert(0, new ListItem("--- בחר ---", "0"));

                    subTypeIdPanel.Visible = true;

                    //string strConn = ConfigurationManager.ConnectionStrings["WebApplicationConnectionString"].ConnectionString;
                    //using (SqlConnection con = new SqlConnection(strConn))
                    //{
                    //    con.Open();

                    //    using (SqlCommand cmd = new SqlCommand("select * from LogisticsInfrastructureTypes where parentID = @parentID", con))
                    //    {
                    //        cmd.Parameters.AddWithValue("@parentID", item.typeId);
                    //        SqlDataReader dr = cmd.ExecuteReader();

                    //        subTypeId.DataTextField = "name";
                    //        subTypeId.DataValueField = "ID";
                    //        subTypeId.DataSource = dr;
                    //        subTypeId.DataBind();

                    //        if (item.subTypeId > 0)
                    //            subTypeId.SelectedValue = Convert.ToString(item.subTypeId);

                    //        subTypeId.Items.Insert(0, new ListItem("--- בחר ---", "0"));

                    //        subTypeIdPanel.Visible = true;

                    //        dr.Dispose();
                    //        cmd.Dispose();
                    //    }
                    //}

                    string marker = callMap("marker", 1, item.latitude);
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "CallMarkerAction", marker, true);

                    gvEquipment.DataSource = GetEquipment(id).ToList();
                    gvEquipment.DataBind();

                    //get population
                    if (lbPopulation.Items.Count > 0)
                    {
                        if (item.relatedPopulationId != null && item.relatedPopulationId != "")
                        {
                            string[] arr = item.relatedPopulationId.Split(',');

                            foreach (string i in arr)
                            {
                                lbPopulation.Items.FindByValue(i).Selected = true;
                            }
                        }
                    }
                    //end 

                    //get images
                    lvImages.DataSource = GetMedia(id).ToList();
                    lvImages.DataBind();
                    //end
                }
            }

            if (action == "Insert")
            {
                updateItemTitleModal.Text = "הוספת תשתית";

                saveButton.CommandName = "Insert";
                saveButton.Text = "הוספת תשתית";

                itemId.Text = Convert.ToString(0);
                typeId.SelectedValue = Convert.ToString(0);
                subTypeId.SelectedValue = Convert.ToString(0);
                subTypeIdPanel.Visible = false;
                name.Text = "";

                int defaultCity = citiesList[0];

                if (citiesCount == 1)
                {
                    ddlCityId.SelectedValue = Convert.ToString(defaultCity);
                    //ddlCityId.Enabled = false;
                    //cityPanel.Style.Add("display", "none");

                    ddlStreetId.DataSource = GetStreets("View", defaultCity).ToList();
                    ddlStreetId.DataBind();
                }
                else
                {
                    ddlCityId.SelectedValue = Convert.ToString(0); 
                    
                    ddlStreetId.DataSource = "";
                    ddlStreetId.DataBind();
                }

                //get map
                string latLng = "";

                var itemCities = (from p in _db.Cities
                                  where p.SignId == defaultCity
                                  select p).SingleOrDefault();

                latLng = itemCities.Lat + "," + itemCities.Lng;

                string marker = callMap("marker", 3, latLng);
                ScriptManager.RegisterStartupScript(this, this.GetType(), "CallMarkerAction", marker, true);
                //end

                street.Text = "";
                streetNumber.Text = "";
                apartmentNumber.Text = "";

                //gvEquipment.DataSource = GetEquipment(0).ToList();
                //gvEquipment.DataBind();

                nameEquipment.Text = "";
                amountRecommendedEquipment.Text = "";
                amountFoundEquipment.Text = "";
            }

            return item;
        }

        protected void addEditInfrastructure_Click(object sender, CommandEventArgs e)
        {
            var action = e.CommandName;
            LogisticsActions logistics = new LogisticsActions();

            string relatedPopulationString = "";

            foreach (ListItem lbPopulationItem in lbPopulation.Items)
            {
                if (lbPopulationItem.Selected)
                {
                    relatedPopulationString += Convert.ToString(lbPopulationItem.Value) + ",";
                }
            }

            relatedPopulationString = relatedPopulationString.TrimEnd(',');

            bool actionSuccess = logistics.AddInfrastructure(action, Convert.ToInt32(infrastructureId.Text), Convert.ToInt32(typeId.SelectedValue), Convert.ToInt32(subTypeId.SelectedValue), name.Text, Convert.ToInt32(ddlCityId.SelectedValue), city.Text, Convert.ToInt32(ddlStreetId.SelectedValue), street.Text, streetNumber.Text, apartmentNumber.Text, markerDetails.Text, polygonDetails.Text, relatedPopulationString);

            if (actionSuccess)
            {
                if (action == "Edit")
                {
                    showMessageBox = ShowMessageBoxAlert(true, "alert alert-success", "פריט נשמר בהצלחה");
                }
                else if (action == "Insert")
                {
                    showMessageBox = ShowMessageBoxAlert(true, "alert alert-success", "פריט נשמר בהצלחה");
                }
                else if (action == "Delete")
                {
                    showMessageBox = ShowMessageBoxAlert(true, "alert alert-success", "פריט נמחק בהצלחה");
                }

                gvLogisticsInfrastructure.DataSource = GetLogisticsInfrastructure(false).ToList();
                gvLogisticsInfrastructure.DataBind();
                gvLogisticsInfrastructure.UseAccessibleHeader = true;
            }
            else
            {
                showMessageBox = ShowMessageBoxAlert(true, "alert alert-error", "אירעה שגיאה במהלך ביצוע הפעולה, אנא נסה שנית");
            }
        }

        protected void GetEquipmentItem_Click(object sender, CommandEventArgs e)
        {
            int id = Convert.ToInt32(e.CommandArgument);
            string action = Convert.ToString(e.CommandName);

            if (action == "EditItem")
            {
                GetEquipmentItem("Edit", id);
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowAccordionEditAction", "$(function() { ChangeClass('#addEquipment', 'accordion-body collapse', 'accordion-body collapse in'); });", true);
            }
        }

        protected void GetEquipmentItem(string action, int? id)
        {
            //var item = _db.LogisticsEquipment.Find(id);
            var item = (from p in _db.LogisticsEquipment
                        where p.ID == id
                        select new
                        {
                            ID = p.ID,
                            amountFoundEquipment = p.amountFound,
                            name = p.logisticsEquipmentType.name,
                            amountRecommended = p.logisticsEquipmentType.amountRecommended,
                            typeId = p.logisticsEquipmentType.ID,
                            statusId = p.logisticsEquipmentType.typeId
                        }).SingleOrDefault();

            infrastructureId.Text = Convert.ToString(id);
            showMessageBox = ShowMessageBoxAlert(false, "", "");

            addInfrastructureAlertPanel.Visible = false;
            addInfrastructureAlertPanel.CssClass = "";
            addInfrastructureAlertLabel.Text = "";

            if (item != null)
            {
                if (action == "View")
                {

                }
                else if (action == "Edit")
                {
                    saveEquipmentButton.CommandName = "Edit";
                    saveEquipmentButton.CommandArgument = Convert.ToString(id);
                    saveEquipmentButton.Text = "ערוך ציוד";

                    deleteEquipmentButton.Visible = true;
                    deleteEquipmentButton.CommandArgument = Convert.ToString(id);

                    itemIdEquipmentType.Text = Convert.ToString(item.typeId);
                    nameEquipment.Text = item.name;
                    amountRecommendedEquipment.Text = Convert.ToString(item.amountRecommended);
                    amountFoundEquipment.Text = Convert.ToString(item.amountFoundEquipment);

                    if (item.statusId == 0)
                    {
                        nameEquipment.Enabled = false;
                        amountRecommendedEquipment.Enabled = false;
                    }
                    else if (item.statusId == 1)
                    {
                        nameEquipment.Enabled = true;
                        amountRecommendedEquipment.Enabled = true;
                    }
                }
            }
        }

        public class PseudoLogisticsEquipmentListItems : LogisticsEquipment
        {
            public string logisticsStructureName { get; set; }
            public string equipmentManager { get; set; }
        }

        public IQueryable<LogisticsEquipment> GetEquipment(int? id)
        {
            DropDownList ddlSubTypeId = this.Master.FindControl("MainContent").FindControl("subTypeId") as DropDownList;
            int checkDdlCityId = Convert.ToInt32(ddlCityId.SelectedValue);

            int GetUserClient = _sa.UserClient(); //get user admin client id

            var result = _db.LogisticsEquipment.AsQueryable();

            if (id != null)
            {
                result = (from p in _db.LogisticsEquipment
                          join li in _db.LogisticsInfrastructures on p.logisticsInfrastructure.ID equals li.ID
                          join le in _db.LogisticsEquipmentTypes on p.logisticsEquipmentType.ID equals le.ID
                          where p.statusId != 2 //not deleted
                          && le.statusId != 2 //not deleted
                          && li.statusId != 2 //not deleted
                          && p.logisticsInfrastructure.ID == id
                          orderby p.ID ascending
                          select new PseudoLogisticsEquipmentListItems()
                            {
                                ID = p.ID,
                                amountFound = p.amountFound,
                                operetor = p.operetor,
                                comment = p.logisticsEquipmentType.name,
                                amountRecommended = p.logisticsEquipmentType.amountRecommended,
                                equipmentManager = (from v in _db.TeamsLevels
                                                    join po in _db.Populations on v.levelManagerId equals po.ID
                                                    where v.status == 1
                                                    && v.team.status == 1
                                                    && v.team.client.ID == GetUserClient
                                                    && v.team.teamType.ID == 1 //צח"י
                                                    && v.team.defaultTeam == true
                                                    && v.teamLevelType.ID == 1 //לוגיסטיקה
                                                    //&& v.team.cityId == li.location.cityKey.SignId //filter cities
                                                    && v.team.cityId == checkDdlCityId //filter cities
                                                    select po.firstName + " " + po.lastName).FirstOrDefault(),
                            });

                if (result.Count() > 0)
                    saveAllEquipmentButton.Visible = true;
                else
                    saveAllEquipmentButton.Visible = false;
            }

            return result;
        }

        public class PseudoLogisticsEquipmentType : LogisticsEquipmentType
        {
            public double amountFound { get; set; }
            public string operetor { get; set; }
            public string comment { get; set; }
        }

        public IQueryable<LogisticsEquipmentType> GetEquipmentTypes(int? id)
        {
            var result = _db.LogisticsEquipmentTypes.AsQueryable();

            if (id != null)
            {
                result = (from p in _db.LogisticsEquipmentTypes
                          where p.parentId == id
                          && p.statusId == 1 //active
                          && p.typeId == 0 //default
                          orderby p.ID ascending
                          select new PseudoLogisticsEquipmentType()
                          {
                              ID = p.ID,
                              amountFound = 0,
                              operetor = "",
                              comment = p.name,
                              amountRecommended = p.amountRecommended
                          });

                if (result.Count() > 0)
                    saveAllEquipmentButton.Visible = true;
                else
                    saveAllEquipmentButton.Visible = false;
            }

            return result;
        }

        protected void gvEquipment_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {

            }
        }

        protected void LogisticsEquipment_Click(object sender, CommandEventArgs e)
        {

        }

        protected void viewAddEquipment_Click(object sender, CommandEventArgs e)
        {
            var action = e.CommandName;

            if (action == "Insert")
            {
                addEquipmentPanel.Visible = true;
                listEquipmentPanel.Visible = false;
            }
            else if (action == "List")
            {
                addEquipmentPanel.Visible = false;
                listEquipmentPanel.Visible = true;
            }
        }

        protected void AddEquipment_Click(object sender, CommandEventArgs e)
        {
            var id = Convert.ToInt32(e.CommandArgument);
            var action = Convert.ToString(e.CommandName);

            LogisticsActions logistics = new LogisticsActions();

            bool actionSuccess = logistics.AddEquipment(action, Convert.ToInt32(itemId.Text), id, Convert.ToInt32(itemIdEquipmentType.Text), Convert.ToInt32(subTypeId.SelectedValue), nameEquipment.Text, Convert.ToDouble(amountRecommendedEquipment.Text), Convert.ToDouble(amountFoundEquipment.Text), 1, 1);

            if (actionSuccess)
            {
                addInfrastructureAlertPanel.Visible = true;
                addInfrastructureAlertPanel.CssClass = "alert alert-success";
                addInfrastructureAlertLabel.Text = "פריט נשמר בהצלחה";

                nameEquipment.Text = "";
                amountRecommendedEquipment.Text = "";
                amountFoundEquipment.Text = "";

                nameEquipment.Enabled = true;
                amountRecommendedEquipment.Enabled = true;

                saveEquipmentButton.Text = "הוסף ציוד";
                saveEquipmentButton.CommandName = "Insert";
                saveEquipmentButton.CommandArgument = "0";
                deleteEquipmentButton.Visible = false;

                gvEquipment.DataSource = GetEquipment(Convert.ToInt32(itemId.Text)).ToList();
                gvEquipment.DataBind();

                equipmentUpdatePanel.Update();
            }
            else
            {
                //labelStatus.Text = "אירעה שגיאה במהלך הוספת הנתונים, אנא נסה שנית";
            }
        }

        protected void EditAllEquipment_Click(object sender, CommandEventArgs e)
        {
            var action = e.CommandName;
            LogisticsActions logistics = new LogisticsActions();
            bool actionSuccess = false;

            if (action == "EditAll")
            {
                foreach (GridViewRow row in gvEquipment.Rows)
                {
                    int equipmentId = Convert.ToInt32((row.FindControl("equipmentId") as Label).Text);
                    TextBox equipmentAmountFound = (TextBox)row.FindControl("equipmentAmountFound");
                    TextBox equipmentOperetor = (TextBox)row.FindControl("equipmentOperetor");

                    actionSuccess = logistics.EditEquipment(action, equipmentId, equipmentAmountFound.Text, equipmentOperetor.Text);
                }
            }

            if (actionSuccess)
            {
                addEquipmentPanel.Visible = false;
                listEquipmentPanel.Visible = true;

                gvEquipment.DataSource = GetEquipment(Convert.ToInt32(infrastructureId.Text)).ToList();
                gvEquipment.DataBind();

                addInfrastructureAlertPanel.Visible = true;
                addInfrastructureAlertPanel.CssClass = "alert alert-success";
                addInfrastructureAlertLabel.Text = "פריט נשמר בהצלחה";
            }
            else
            {
                addInfrastructureAlertPanel.Visible = true;
                addInfrastructureAlertPanel.CssClass = "alert alert-error";
                addInfrastructureAlertLabel.Text = "אירעה שגיאה במהלך ביצוע הפעולה, אנא נסה שנית";
            }
        }

        protected void ClearMap_Click(object sender, CommandEventArgs e)
        {
            int cityId = Convert.ToInt32(ddlCityId.SelectedValue);

            var query = _db.Cities.AsQueryable();
            var lat = "";
            var lng = "";

            if (cityId == 0)
            {
                //get cities
                List<int> citiesList = new List<int>();

                SystemActions _sa = new SystemActions();
                string GetUserCities = _sa.UserCities();

                if (GetUserCities != null)
                {
                    if (GetUserCities != "")
                    {
                        string[] arr = GetUserCities.Split(',');

                        foreach (string i in arr)
                        {
                            int checkId = Convert.ToInt32(i);

                            var itemCities = (from p in _db.Cities
                                              where p.SignId == checkId
                                              select p).SingleOrDefault();

                            lat = itemCities.Lat;
                            lng = itemCities.Lng;
                        }
                    }
                }
                //end
            }
            else
            {
                var itemCities = (from p in _db.Cities
                                  where p.SignId == cityId
                                  select p).SingleOrDefault();

                lat = itemCities.Lat;
                lng = itemCities.Lng;
            }

            string marker = "";
            marker += "function callMarker() {";
            marker += "var latLng = new google.maps.LatLng(" + lat + "," + lng + ");";
            marker += "map = new google.maps.Map(document.getElementById('main-map'), {";
            marker += "zoom: 12,";
            marker += "center: latLng,";
            marker += "mapTypeId: google.maps.MapTypeId.ROADMAP,";
            marker += "disableDefaultUI: true,";
            marker += "zoomControl: true";
            marker += "});";
            marker += "}";

            js.Text = "";
            js.Text += "<script type='text/javascript'>";
            js.Text += marker;
            js.Text += "</script> ";
        }

        public class PseudoLogisticsMediaItems : Media
        {
            public string fileName { get; set; }
            public int clientId { get; set; }      
        }

        public IEnumerable<LogisticsInfrastructure> GetLogisticsMap(bool? filter = false)
        {
            var action = Convert.ToString(Page.RouteData.Values["action"]);
            int id = Convert.ToInt32(Page.RouteData.Values["id"]);
            string pageTitleString = "";

            int GetUserClient = _sa.UserClient(); //get user admin client id

            //filter admin city
            List<int> citiesList = new List<int>();
            string GetUserCities = _sa.UserCities();

            if (GetUserCities != null)
            {
                if (GetUserCities != "")
                {
                    string[] arr = GetUserCities.Split(',');

                    foreach (string i in arr)
                    {
                        citiesList.Add(Convert.ToInt32(i));
                    }
                }
            }
            //end

            var query = _db.LogisticsInfrastructures.AsEnumerable(); //IEnumerable

            query = (from p in _db.LogisticsInfrastructures
                     where p.client.ID == GetUserClient
                     && citiesList.Contains(p.location.cityKey.SignId)
                     && p.statusId != 2 //0 = not active, 1 = active, 2 = deleted
                     && p.location.latitude != ""
                     select new PseudoItems()
                     {
                         ID = p.ID,
                         name = p.name,
                         typeId = p.typeId,
                         subTypeId = p.subTypeId,
                         //location
                         cityId = p.location.cityKey.SignId,
                         Street = p.location.Street,
                         streetNumber = p.location.streetNumber,
                         apartmentNumber = p.location.apartmentNumber,
                         locationId = (p.location.ID == null) ? 0 : p.location.ID,
                         //end 
                         //map
                         address = (p.location.streetKey.Name != null ? (p.location.streetKey.Name + (p.location.streetNumber != "" ? " " + p.location.streetNumber : "") + (p.location.apartmentNumber != "" ? " (" + p.location.apartmentNumber + ")" : "") + ", ") : "") + (p.location.cityKey.Name != null ? p.location.cityKey.Name : ""),
                         latitude = p.location.latitude
                         //end  
                     }).ToList();

            if (filter == true)
            {
                pageTitle.Text = "תשתיות - תוצאות חיפוש פילוח";

                int typeIdCheck = Convert.ToInt32(filterTypeId.SelectedValue);
                int subTypeIdCheck = 0;

                if (filterSubTypeId.Items.FindByValue("0") != null)
                    subTypeIdCheck = Convert.ToInt32(filterSubTypeId.SelectedValue);

                string nameCheck = nameFilter.Text;
                int cityIdCheck = Convert.ToInt32(cityIdFilter.SelectedValue);
                string streetCheck = Convert.ToString(streetFilter.Text);
                string streetNumberCheck = Convert.ToString(streetNumberFilter.Text);
                string apartmentNumberCheck = Convert.ToString(apartmentNumberFilter.Text);

                if (streetCheck != null)
                    streetCheck = streetCheck.Replace('+', ' ');

                //add the filter parameters
                if (typeIdCheck != 0)
                    query = query.Where(c => c.typeId == typeIdCheck);

                if (subTypeIdCheck != 0)
                    query = query.Where(c => c.subTypeId == subTypeIdCheck);

                if (nameCheck != "")
                    query = query.Where(c => c.name == nameCheck);

                if (cityIdCheck != 0)
                    query = query.Where(c => c.cityId == cityIdCheck);

                if (streetCheck != "")
                    query = query.Where(c => c.Street == streetCheck);

                if (streetNumberCheck != "")
                    query = query.Where(c => c.streetNumber == streetNumberCheck);

                if (apartmentNumberCheck != "")
                    query = query.Where(c => c.apartmentNumber == apartmentNumberCheck);
                //end
            }

            if (id != 0)
            {
                query = query.Where(c => c.typeId == id);

                var queryType = (from p in _db.LogisticsInfrastructureTypes
                              where p.ID == id
                              select new
                              {
                                  name = p.name
                              }).SingleOrDefault();

                pageTitleString = "תשתיות - " + queryType.name;
            }
            else
            {
                pageTitleString = "תשתיות - תצוגת מפה";
            }

            pageTitle.Text = pageTitleString;
            tableTitle.Text = pageTitleString;

            var result = (from p in query
                          select p).ToList();

            //map
            var locationsList = new List<string>();
            var infoWindowContentsList = new List<string>();

            foreach (var i in result)
            {
                string mediaImages = "";

                var media = (from m in _db.Media
                             where m.logisticsInfrastructure.ID == i.ID
                             && m.statusId == 1 //active
                             && m.fileName != ""
                             select new PseudoLogisticsMediaItems()
                             {
                                 fileName = m.fileName,
                                 clientId = m.client.ID
                             }).ToList();

                if (media != null)
                {
                    foreach (var x in media)
                    {
                        string getMediaImages = x.fileName;
                        int getClientId = x.clientId;

                        mediaImages += "<div style='float: right; padding-left: 10px; padding-bottom: 10px;'><img src='/Uploads/" + getClientId + "/" + getMediaImages + "' width='150px' /></div>";
                    }
                }

                var location = (from s in _db.Locations
                                where s.parentId == i.ID
                                && s.typeId == 2
                                && s.latitude != ""
                                && s.cityKey.ID != null
                                && s.streetKey.ID != null
                                select new
                                {
                                    latitude = s.latitude,
                                    address = (s.streetKey.Name != null ? (s.streetKey.Name + (s.streetNumber != "" ? " " + s.streetNumber : "") + (s.apartmentNumber != "" ? " (" + s.apartmentNumber + ")" : "") + ", ") : "") + (s.cityKey.Name != null ? s.cityKey.Name : "")
                                }).FirstOrDefault();

                if (location != null)
                {
                    locationsList.Add(string.Format(
                                @"{{ 
                                                position: new google.maps.LatLng({0})
                                            }}",
                                   location.latitude
                                )
                              );

                    infoWindowContentsList.Add(string.Format(
                            @"{{ 
                                                content: ""<div class='infoWindow' style='width:350px; height:250px; overflow:auto;'><div style='padding-top:5px; font-weight: bold;'>{1}</div><div>כתובת: {2}</div><div style='margin-left:5px; border-top: 1px solid #ccc;'></div><div style='padding-top: 10px;'>{3}</div></div>""
                                            }}",
                               i.ID,
                               i.name,
                               HttpUtility.HtmlEncode(location.address),                        
                               mediaImages
                            )
                       );
                }
                //end

                //content: ""<div class='infoWindow'><table border='0'><tr><td valign='top'><b>{0}</b><br />כתובת: {1} {2}<br /><a href='/Account/Logistics/Infrastructure/Edit/{3}' />לצפיה בתשתית</a></td><td valign='top'><img src='/Uploads/{4}' width='200px'></td></tr></table></div>""
                //Response.Write("#" + i.name + "#" + location.address + "#" + i.ID + "<br /><br />");
            }

            var lat = "31.964014";
            var lng = "34.79797600000006";
            var locationsJsonList = "[" + string.Join(",", locationsList.ToArray()) + "]";
            var overlayContentsJsonList = "[" + string.Join(",", infoWindowContentsList.ToArray()) + "]";

            //Response.Write("#" + locationsJsonList + "#" + overlayContentsJsonList + "<br />");

            ScriptManager.RegisterStartupScript(this.Page, GetType(), "Google Maps Initialization",
                string.Format("init_map('main-map-big', {0}, {1}, 14, {2}, {3});", lat, lng, locationsJsonList, overlayContentsJsonList), true);

            //return result;
            return query;
        }

        void ImageButton_OnCommand(object sender, EventArgs e)
        {
            // Handle image button command event
        }

        protected void ChangeViewButton_Click(object sender, CommandEventArgs e)
        {
            string action = Convert.ToString(e.CommandName);

            if (action == "Map")
            {
                GetLogisticsMap(false).ToList();

                mapMapViewPanel.Visible = true;
                //mapListViewPanel.Visible = false;
                logisticsInfrastructureMapPanel.Visible = true;
                logisticsInfrastructureListPanel.Visible = false;

                changeViewLinkButton.Text = "תצוגת רשימה";
                changeViewLinkButton.CommandName = "List";
            }
            else if (action == "List")
            {
                gvLogisticsInfrastructure.DataSource = GetLogisticsInfrastructure(false).ToList();
                gvLogisticsInfrastructure.DataBind();
                gvLogisticsInfrastructure.UseAccessibleHeader = true;

                mapMapViewPanel.Visible = false;
                //mapListViewPanel.Visible = true;
                logisticsInfrastructureMapPanel.Visible = false;
                logisticsInfrastructureListPanel.Visible = true;

                changeViewLinkButton.Text = "תצוגת מפה";
                changeViewLinkButton.CommandName = "Map";
            }

            HttpCookie pageView = new HttpCookie("pageViewCookie");
            DateTime now = DateTime.Now;

            pageView.Value = action.ToString(); //set the cookie value.
            pageView.Expires = now.AddHours(1);  //set the cookie expiration date.
            Response.Cookies.Add(pageView); //add the cookie.
        }

        public string callMap(string action, int type, string latLng)
        {
            string marker = "";
            //type = 1 for map with marker, 2 for city with marker, 3 for city with no marker

            //start map
            if (action == "marker")
            {
                marker += "function callMarker() {";

                marker += "var latLngPoint = new google.maps.LatLng(" + latLng + ");";
                marker += "map.setCenter(latLngPoint);";

                if (type == 1)
                {
                    marker += "marker = new google.maps.Marker({";
                    marker += "zoom: 20,";
                    marker += "zoomControl: true,";
                    marker += "map: map,";
                    marker += "draggable: true,";
                    marker += "animation: google.maps.Animation.DROP,";
                    marker += "position: latLngPoint";
                    marker += "});";
                    marker += "markersArray.push(marker);";

                    marker += "updateMarkerPosition(marker.getPosition());";
                    marker += "geocodePosition(marker.getPosition());";
                }
            }
            else if (action == "city")
            {
                marker += "$(function() {";

                marker += "var latLngPoint = new google.maps.LatLng(" + latLng + ");";
                marker += "map.setCenter(latLngPoint);";

                marker += "map = new google.maps.Map(document.getElementById('main-map'), {";
                marker += "zoom: 12,";
                marker += "zoomControl: true,";
                marker += "center: latLngPoint,";
                marker += "mapTypeId: google.maps.MapTypeId.ROADMAP,";
                marker += "disableDefaultUI: true";
                marker += "});";

                marker += "marker = new google.maps.Marker({";
                marker += "zoom: 20,";
                marker += "zoomControl: true,";
                marker += "map: map,";
                marker += "draggable: true,";
                marker += "animation: google.maps.Animation.DROP,";
                marker += "position: latLngPoint";
                marker += "});";
                marker += "markersArray.push(marker);";

                marker += "updateMarkerPosition(marker.getPosition());";
                marker += "geocodePosition(marker.getPosition());";
            }

            //add dragging event listeners
            marker += "google.maps.event.addListener(marker, 'dragstart', function () {";
            marker += "updateMarkerAddress('Dragging...');";
            marker += "});";

            marker += "google.maps.event.addListener(marker, 'drag', function () {";
            marker += "updateMarkerStatus('Dragging...');";
            marker += "updateMarkerPosition(marker.getPosition());";
            marker += "});";

            marker += "google.maps.event.addListener(marker, 'dragend', function () {";
            marker += "updateMarkerStatus('Drag ended');";
            marker += "geocodePosition(marker.getPosition());";
            marker += "});";
            //end

            //marker += "alert('2');";

            if (action == "marker")
            {
                marker += "}";
            }
            else if (action == "city")
            {
                marker += "});";
            }

            return marker;
        }

        public class PseudoMedia : Media
        {
            public string fileName { get; set; }
            public int clientId { get; set; }
            public int infrastructureId { get; set; }
        }

        public IQueryable<Media> GetMedia(int? id)
        {
            var result = _db.Media.AsQueryable();

            result = (from p in _db.Media
                      where p.statusId == 1 //active
                      && p.logisticsInfrastructure.ID == id
                      orderby p.ID ascending
                      select new PseudoMedia()
                      {
                          ID = p.ID,
                          name = p.name,
                          fileName = p.fileName,
                          clientId = p.client.ID
                      });

            if (result.Count() > 0)
                lvImages.Visible = true;
            else
                lvImages.Visible = false;

            return result;
        }

        protected void AsyncFileUploadUploadedComplete(object sender, AsyncFileUploadEventArgs e)
        {
            int id = Convert.ToInt32(infrastructureId.Text);
            var asyncFileUpload = (AsyncFileUpload)sender;

            string columnName = Convert.ToString(asyncFileUpload.ClientID);
            //columnName = columnName.Replace("MainContent_AsyncFileUpload_", "");
            string fileName = _cf.RandomCodeLong();
            int GetUserClient = _sa.UserClient();

            if (asyncFileUpload.HasFile)
            {
                Path.GetFileName(e.FileName);
                var extension = Path.GetExtension(asyncFileUpload.PostedFile.FileName);
                var uploadFileName = fileName + extension;

                var savePath = MapPath("~/Uploads/" + GetUserClient); //path        

                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }

                asyncFileUpload.SaveAs(savePath + "/" + uploadFileName); //save file

                AsyncFileUploadUpdate("Insert", 0, 1, id, uploadFileName); //typeId = 1 > logistics infrastructure

                ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowAction", "top.$get('uploadResult').innerHTML='קובץ נוסף בהצלחה, לחץ על רענן רשימת תמונות'; top.$get('MainContent_refreshImagesButton').style.display = 'block';", true);
            }
        }

        protected void AsyncFileUploadUpdate(string action, int itemId, int typeId, int typeItemId, string fileName)
        {
            MediaActions media = new MediaActions();

            bool actionSuccess = media.AddMedia(action, itemId, typeId, typeItemId, fileName);

            if (actionSuccess)
            {
                //get images
                lvImages.DataSource = GetMedia(typeItemId).ToList();
                lvImages.DataBind();
                //end
            }
        }

        protected void addEditImage_Click(object sender, CommandEventArgs e)
        {
            int itemId = Convert.ToInt32(infrastructureId.Text);
            int id = Convert.ToInt32(e.CommandArgument);
            var action = e.CommandName;

            MediaActions media = new MediaActions();

            var item = (from p in _db.Media
                        where p.ID == id
                        select new PseudoMedia()
                        {
                            ID = p.ID,
                            fileName = p.fileName,
                            infrastructureId = p.logisticsInfrastructure.ID
                        }).SingleOrDefault();

            if (item != null)
            {
                bool actionSuccess = media.AddMedia(action, id, 1, item.infrastructureId, item.fileName);

                if (actionSuccess)
                {
                    lvImages.DataSource = GetMedia(itemId).ToList();
                    lvImages.DataBind();

                    imagesUpdatePanel.Update();
                }
                else
                {
                    showMessageBox = ShowMessageBoxAlert(true, "alert alert-error", "אירעה שגיאה במהלך ביצוע הפעולה, אנא נסה שנית");
                }
            }
        }

        protected void refreshImages_Click(object sender, CommandEventArgs e)
        {
            int itemId = Convert.ToInt32(infrastructureId.Text);

            lvImages.DataSource = GetMedia(itemId).ToList();
            lvImages.DataBind();

            imagesUpdatePanel.Update();
        }

        public bool ShowMessageBoxAlert(bool show, string css, string text)
        {
            bool success = true;

            alertPanel.Visible = show;
            alertPanel.CssClass = css;
            messageLabel.Text = text;

            return success;
        }
    }
}