using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using VTSServices.Models;

namespace VTSServices.Controllers
{
    public class UserController : ApiController
    {
        string constr = ConfigurationManager.ConnectionStrings["MyConn"].ConnectionString;

        [HttpPost]
        public HttpResponseMessage AddUser([FromBody] User user)
        {
            try
            {
                SqlConnection con = new SqlConnection(constr);
                SqlCommand cmd = new SqlCommand("InsertUserDeatails", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Name", user.Name);
                cmd.Parameters.AddWithValue("@MobileNumber", user.MobileNumber);
                cmd.Parameters.AddWithValue("@Organization", user.Organization);
                cmd.Parameters.AddWithValue("@Address", user.Address);
                cmd.Parameters.AddWithValue("@Emailaddress", user.Emailaddress);
                cmd.Parameters.AddWithValue("@Location", user.Location);
                con.Open();
                int rowInserted = cmd.ExecuteNonQuery();
                var message = Request.CreateResponse(HttpStatusCode.Created, user);
                return message;
                //con.Close();
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpPut]
        public HttpResponseMessage Put(int id, [FromBody]User user)
        {
            try
            {
                SqlConnection con = new SqlConnection(constr);
                SqlCommand cmd = new SqlCommand("UpdateUserDeatails", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserId", id);
                cmd.Parameters.AddWithValue("@Name", user.Name);
                cmd.Parameters.AddWithValue("@MobileNumber", user.MobileNumber);
                cmd.Parameters.AddWithValue("@Organization", user.Organization);
                cmd.Parameters.AddWithValue("@Address", user.Address);
                cmd.Parameters.AddWithValue("@Emailaddress", user.Emailaddress);
                cmd.Parameters.AddWithValue("@Location", user.Location);
                con.Open();
                int rowUpdated = cmd.ExecuteNonQuery();

                if (rowUpdated != 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, user);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "User with id " + id + " is not found");
                }

                //con.Close();
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }



        [Route("api/user/PostUserImage/{Id}")]
        public HttpResponseMessage PostUserImage(int Id)
        {
            if (Id > 0)
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                try
                {

                    var httpRequest = HttpContext.Current.Request;

                    foreach (string file in httpRequest.Files)
                    {
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created);

                        var postedFile = httpRequest.Files[file];
                        if (postedFile != null && postedFile.ContentLength > 0)
                        {

                            int MaxContentLength = 1024 * 1024 * 1; //Size = 1 MB  

                            IList<string> AllowedFileExtensions = new List<string> { ".jpg", ".gif", ".png" };
                            var ext = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf('.'));
                            var extension = ext.ToLower();
                            if (!AllowedFileExtensions.Contains(extension))
                            {

                                var message = string.Format("Please Upload image of type .jpg,.gif,.png.");

                                dict.Add("error", message);
                                return Request.CreateResponse(HttpStatusCode.BadRequest, dict);
                            }
                            else if (postedFile.ContentLength > MaxContentLength)
                            {

                                var message = string.Format("Please Upload a file upto 1 mb.");

                                dict.Add("error", message);
                                return Request.CreateResponse(HttpStatusCode.BadRequest, dict);
                            }
                            else
                            {



                                var filePath = HttpContext.Current.Server.MapPath("~/Userimage/" + postedFile.FileName + "_" + Id + extension);

                                postedFile.SaveAs(filePath);

                                using (SqlConnection con = new SqlConnection(constr))
                                {
                                    using (SqlCommand cmd = new SqlCommand("UploadProfilePic"))
                                    {
                                        using (SqlDataAdapter sda = new SqlDataAdapter())
                                        {
                                            cmd.CommandType = CommandType.StoredProcedure;
                                            cmd.Parameters.AddWithValue("@UserId", Id);
                                            cmd.Parameters.AddWithValue("@Path", "~/Userimage/" + Id + "_" + postedFile.FileName);
                                            cmd.Connection = con;
                                            con.Open();
                                            cmd.ExecuteNonQuery();
                                            // con.Close();
                                        }
                                    }

                                }

                            }
                        }

                        var message1 = string.Format("Image Updated Successfully.");
                        return Request.CreateErrorResponse(HttpStatusCode.Created, message1); ;
                    }
                    var res = string.Format("Please Upload a image.");
                    dict.Add("error", res);
                    return Request.CreateResponse(HttpStatusCode.NotFound, dict);
                }
                catch (Exception ex)
                {
                    var res = string.Format("some Message");
                    dict.Add("error", res);
                    return Request.CreateResponse(HttpStatusCode.NotFound, dict);
                }
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, "No Id found");
            }

        }

        [HttpGet]
        public HttpResponseMessage GetCustomer([FromUri]PagingParameterModel pagingparametermodel)
        {
            var source = TestVehicleList(pagingparametermodel.QuerySearch);

            int count = source.Count();

            // Parameter is passed from Query string if it is null then it default Value will be pageNumber:1  
            int CurrentPage = pagingparametermodel.pageNumber;
            if (count > 0)
            {

                // Parameter is passed from Query string if it is null then it default Value will be pageSize:20  
                int PageSize = pagingparametermodel.pageSize;

                // Display TotalCount to Records to User  
                int TotalCount = count;

                // Calculating Totalpage by Dividing (No of Records / Pagesize)  
                int TotalPages = (int)Math.Ceiling(count / (double)PageSize);

                // Returns List of Customer after applying Paging   
                var items = source.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();

                // if CurrentPage is greater than 1 means it has previousPage  
                var previousPage = CurrentPage > 1 ? "Yes" : "No";

                // if TotalPages is greater than CurrentPage means it has nextPage  
                var nextPage = CurrentPage < TotalPages ? "Yes" : "No";

                // Object which we are going to send in header   
                var paginationMetadata = new
                {
                    totalCount = TotalCount,
                    pageSize = PageSize,
                    currentPage = CurrentPage,
                    totalPages = TotalPages,
                    previousPage,
                    nextPage,
                    QuerySearch = string.IsNullOrEmpty(pagingparametermodel.QuerySearch) ?
                                  "No Parameter Passed" : pagingparametermodel.QuerySearch
                };

                // Setting Header  
                HttpContext.Current.Response.Headers.Add("Paging-Headers", JsonConvert.SerializeObject(paginationMetadata));
                // Returing List of Customers Collections  
                //return items;
                return Request.CreateResponse(HttpStatusCode.OK, items);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, "Not Data fount with search parameter " + pagingparametermodel.QuerySearch);
            }

            //return Request.CreateResponse(HttpStatusCode.OK, "100");

        }

        public IEnumerable<Vehicle> TestVehicleList(string searchParameter)
        {

            using (SqlConnection sqlConnection = new SqlConnection(constr))
            {

                using (SqlCommand cmd = new SqlCommand("SearchVehicle"))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@searchParameter", searchParameter);
                    cmd.Connection = sqlConnection;
                    sqlConnection.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Create a Favorites instance
                            var favorites = new Vehicle();
                            favorites.VehicleNumber = Convert.ToInt32(reader["VehicleNumber"]);
                            favorites.VehicleType = reader["VehicleType"].ToString();
                            favorites.ChassisNumber = reader["ChassisNumber"].ToString();
                            favorites.EngineNumber = reader["EngineNumber"].ToString();
                            favorites.Manufacturingyear = reader["Manufacturingyear"].ToString();
                            favorites.Loadcarryingcapacity = reader["Loadcarryingcapacity"].ToString();
                            favorites.Makeofvehicle = reader["Makeofvehicle"].ToString();
                            favorites.ModelNumber = reader["ModelNumber"].ToString();
                            favorites.Bodytype = reader["Bodytype"].ToString();
                            favorites.Organisationname = reader["Organisationname"].ToString();
                            favorites.DeviceName = reader["DeviceName"].ToString();
                            favorites.UserName = reader["UserName"].ToString();
                            // ... etc ...
                            yield return favorites;
                        }
                    }
                }
            }
        }
    }
}
