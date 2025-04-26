using ApiCallers.MelkavanApiCaller;
using ApiCallers.PublicApiCaller;
using ApiCallers.TeniacoApiCaller;
using AutoMapper;
using CustomAttributes;
using FrameWork;
using Microsoft.Aspnet.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Models.Business.ConsoleBusiness;
using Newtonsoft.Json.Linq;
using Services.Business;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using VM.Base;
using VM.Console;
using VM.Melkavan;
using VM.Melkavan.PVM.Melkavan.Tags;
using VM.Public;
using VM.PVM.Melkavan;
using VM.PVM.Public;
using VM.PVM.Teniaco;
using VM.Teniaco;
using Web.Core.Controllers;

namespace Web.Melkavan.Areas.UserMelkavan.Controllers
{
    [Area("UserMelkavan")]
    public class MelkavanPanelController : ExtraUsersController
    {
        //private string key = "teniaco@orgtenia";

        public MelkavanPanelController(IHostEnvironment _hostEnvironment,
            IHttpContextAccessor _httpContextAccessor,
            IActionContextAccessor _actionContextAccessor,
            IConfigurationRoot _configurationRoot,
            IMapper _mapper,
            IConsoleBusiness _consoleBusiness,
            IPublicServicesBusiness _publicServicesBusiness,
            IMemoryCache _memoryCache,
            IDistributedCache _distributedCache) :
            base(_hostEnvironment,
            _httpContextAccessor,
            _actionContextAccessor,
            _configurationRoot,
            _mapper,
            _consoleBusiness,
            _publicServicesBusiness,
            _memoryCache,
            _distributedCache)
        {
        }



        #region AdvertisementManagement

        [AllowAnonymous]
        [HttpPost]
        [AjaxOnly]
        public JsonResult ChangePriceType(int AdvertisementTypeId)
        {
            int platform;
            // اگر فروش بود
            if (AdvertisementTypeId == 2)
            {
                platform = 3;
            }
            // اگر اجاره بود
            else
            {
                platform = 4;
            }


            JsonResultWithRecordsObjectVM jsonResultWithRecordsObjectVM2 = new JsonResultWithRecordsObjectVM(new object() { });


            string serviceUrl2 = teniacoApiUrl + "/api/MapLayerCategoriesManagement/GetListOfPropertiesPricesForMap";

            GetListOfPropertiesPricesForMapPVM getListOfPropertiesPricesForMapPVM = new GetListOfPropertiesPricesForMapPVM()
            {
                ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                Platform = platform
            };

            responseApiCaller = new TeniacoApiCaller(serviceUrl2).GetListOfPropertiesPricesForMap(getListOfPropertiesPricesForMapPVM);

            if (responseApiCaller.IsSuccessStatusCode)
            {
                jsonResultWithRecordsObjectVM2 = responseApiCaller.JsonResultWithRecordsObjectVM;

                if (jsonResultWithRecordsObjectVM2 != null)
                {
                    if (jsonResultWithRecordsObjectVM2.Result.Equals("OK"))
                    {

                        JArray jArray = jsonResultWithRecordsObjectVM2.Records;
                        var records = jArray.ToObject<List<PropertiesPricesForMapVM>>();


                        return Json(new
                        {
                            Result = jsonResultWithRecordsObjectVM2.Result,
                            Records = records
                        });

                    }
                }
            }

            return Json(new
            {
                Result = "ERROR"
            });
        }



        //[Route("", Name = "melkavan.com")]

        //[Route("", Name = "localhost")]

        //صفحه ی اصلی ملکاوان
        [AllowAnonymous]
        public IActionResult Index()
        {
            // FILLING FILTERS VALUE


            JsonResultWithRecordsObjectVM jsonResultWithRecordsObjectVM2 = new JsonResultWithRecordsObjectVM(new object() { });



            #region Price map

            string serviceUrl2 = teniacoApiUrl + "/api/MapLayerCategoriesManagement/GetListOfPropertiesPricesForMap";

            GetListOfPropertiesPricesForMapPVM getListOfPropertiesPricesForMapPVM = new GetListOfPropertiesPricesForMapPVM()
            {
                ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                Platform = 3
            };

            responseApiCaller = new TeniacoApiCaller(serviceUrl2).GetListOfPropertiesPricesForMap(getListOfPropertiesPricesForMapPVM);

            if (responseApiCaller.IsSuccessStatusCode)
            {
                jsonResultWithRecordsObjectVM2 = responseApiCaller.JsonResultWithRecordsObjectVM;

                if (jsonResultWithRecordsObjectVM2 != null)
                {
                    if (jsonResultWithRecordsObjectVM2.Result.Equals("OK"))
                    {

                        JArray jArray = jsonResultWithRecordsObjectVM2.Records;
                        var records = jArray.ToObject<List<PropertiesPricesForMapVM>>();


                        ViewData["Records"] = records;
                        ViewData["Result"] = jsonResultWithRecordsObjectVM2.Result;

                    }
                }
            }

            #endregion

            string userDevicePlatform = FrameWork.PlatformInfo.IsMobile(Request.Headers["User-Agent"].ToString()).Equals(true) ? "mobile" : "desktop";
            if (ViewData["UserDevicePlatform"] == null)
            {
                ViewData["UserDevicePlatform"] = userDevicePlatform;
            }

            if (ViewData["CurrentYear"] == null)
            {
                ViewData["CurrentYear"] = PersianDate.ThisYear;
            }


            //لیبل ها
            if (ViewData["TagsList"] == null)
            {
                List<TagsVM> tagsList = new List<TagsVM>();

                GetAllTagsListPVM getAllTagsListPVM = new GetAllTagsListPVM()
                {
                    ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                    this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                };


                try
                {
                    serviceUrl = melkavanApiUrl + "/api/TagsManagement/GetAllTagsList";

                    responseApiCaller = new MelkavanApiCaller(serviceUrl).GetAllTagsList(getAllTagsListPVM);

                    if (responseApiCaller.IsSuccessStatusCode)
                    {
                        jsonResultWithRecordsObjectVM2 = responseApiCaller.JsonResultWithRecordsObjectVM;

                        if (jsonResultWithRecordsObjectVM2 != null)
                        {
                            if (jsonResultWithRecordsObjectVM2.Result.Equals("OK"))
                            {
                                JArray jArray = jsonResultWithRecordsObjectVM2.Records;
                                tagsList = jArray.ToObject<List<TagsVM>>();
                            }
                        }
                    }
                }
                catch (Exception exc)
                { }

                ViewData["TagsList"] = tagsList;
            }

            //نوع آگهی
            if (ViewData["AdvertisementTypesList"] == null)
            {
                List<AdvertisementTypesVM> advertisementTypesList = new List<AdvertisementTypesVM>();

                GetAllAdvertisementTypesListPVM getAllAdvertisementTypesListPVM = new GetAllAdvertisementTypesListPVM()
                {
                    ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                    this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                    //ChildsUsersIds = consoleBusiness.GetDomainChildUserIdsWithStoredProcedure(this.userId.Value,
                    //       this.domainsSettings.DomainSettingId),
                };
                try
                {
                    serviceUrl = melkavanApiUrl + "/api/AdvertisementTypesManagement/GetAllAdvertisementTypesList";

                    responseApiCaller = new MelkavanApiCaller(serviceUrl).GetAllAdvertisementTypesList(getAllAdvertisementTypesListPVM);

                    if (responseApiCaller.IsSuccessStatusCode)
                    {
                        jsonResultWithRecordsObjectVM2 = responseApiCaller.JsonResultWithRecordsObjectVM;

                        if (jsonResultWithRecordsObjectVM2 != null)
                        {
                            if (jsonResultWithRecordsObjectVM2.Result.Equals("OK"))
                            {
                                #region Fill UserCreatorName

                                JArray jArray = jsonResultWithRecordsObjectVM2.Records;
                                advertisementTypesList = jArray.ToObject<List<AdvertisementTypesVM>>();



                                #endregion
                            }
                        }
                    }
                }
                catch (Exception exc)
                { }

                ViewData["AdvertisementTypesList"] = advertisementTypesList;
            }


            //نوع ملک
            if (ViewData["PropertyTypesList"] == null)
            {
                List<PropertyTypesVM> propertyTypesVMList = new List<PropertyTypesVM>();

                GetAllPropertyTypesListPVM getAllPropertyTypesListPVM = new GetAllPropertyTypesListPVM()
                {
                    ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                    this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                    //ChildsUsersIds = consoleBusiness.GetDomainChildUserIdsWithStoredProcedure(this.userId.Value,
                    //       this.domainsSettings.DomainSettingId),
                };
                try
                {
                    serviceUrl = teniacoApiUrl + "/api/PropertyTypesManagement/GetAllPropertyTypesList";

                    responseApiCaller = new TeniacoApiCaller(serviceUrl).GetAllPropertyTypesList(getAllPropertyTypesListPVM);

                    if (responseApiCaller.IsSuccessStatusCode)
                    {
                        jsonResultWithRecordsObjectVM2 = responseApiCaller.JsonResultWithRecordsObjectVM;

                        if (jsonResultWithRecordsObjectVM2 != null)
                        {
                            if (jsonResultWithRecordsObjectVM2.Result.Equals("OK"))
                            {
                                #region Fill UserCreatorName

                                JArray jArray = jsonResultWithRecordsObjectVM2.Records;
                                propertyTypesVMList = jArray.ToObject<List<PropertyTypesVM>>();



                                #endregion
                            }
                        }
                    }
                }
                catch (Exception exc)
                { }

                ViewData["PropertyTypesList"] = propertyTypesVMList;
            }


            //نوع کاربری
            if (ViewData["TypeOfUsesList"] == null)
            {
                List<TypeOfUsesVM> typeOfUsesVMList = new List<TypeOfUsesVM>();

                GetAllTypeOfUsesListPVM getAllTypeOfUsesListPVM = new GetAllTypeOfUsesListPVM()
                {
                    ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                    this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                    //ChildsUsersIds = consoleBusiness.GetDomainChildUserIdsWithStoredProcedure(this.userId.Value,
                    //       this.domainsSettings.DomainSettingId),
                };
                try
                {
                    serviceUrl = teniacoApiUrl + "/api/TypeOfUsesManagement/GetAllTypeOfUsesList";

                    responseApiCaller = new TeniacoApiCaller(serviceUrl).GetAllTypeOfUsesList(getAllTypeOfUsesListPVM);

                    if (responseApiCaller.IsSuccessStatusCode)
                    {
                        jsonResultWithRecordsObjectVM2 = responseApiCaller.JsonResultWithRecordsObjectVM;

                        if (jsonResultWithRecordsObjectVM2 != null)
                        {
                            if (jsonResultWithRecordsObjectVM2.Result.Equals("OK"))
                            {
                                #region Fill UserCreatorName

                                JArray jArray = jsonResultWithRecordsObjectVM2.Records;
                                typeOfUsesVMList = jArray.ToObject<List<TypeOfUsesVM>>();

                                #endregion
                            }
                        }
                    }
                }
                catch (Exception exc)
                { }

                ViewData["TypeOfUsesList"] = typeOfUsesVMList;
            }


            //استان
            if (ViewData["StatesList"] == null)
            {
                List<StatesVM> statesVMList = new List<StatesVM>();

                try
                {
                    serviceUrl = publicApiUrl + "/api/StatesManagement/GetListOfStates";

                    GetListOfStatesPVM getListOfStatesPVM = new GetListOfStatesPVM()
                    {
                        //ChildsUsersIds = consoleBusiness.GetDomainChildUserIdsWithStoredProcedure(this.userId.Value,
                        //    this.domainsSettings.DomainSettingId),
                        ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                        this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                    };

                    responseApiCaller = new PublicApiCaller(serviceUrl).GetListOfStates(getListOfStatesPVM);

                    if (responseApiCaller.IsSuccessStatusCode)
                    {
                        jsonResultWithRecordsObjectVM2 = responseApiCaller.JsonResultWithRecordsObjectVM;

                        if (jsonResultWithRecordsObjectVM2 != null)
                        {
                            if (jsonResultWithRecordsObjectVM2.Result.Equals("OK"))
                            {
                                #region Fill UserCreatorName

                                JArray jArray = jsonResultWithRecordsObjectVM2.Records;
                                statesVMList = jArray.ToObject<List<StatesVM>>();


                                #endregion
                            }
                        }
                    }
                }
                catch (Exception exc)
                { }

                ViewData["StatesList"] = statesVMList;
            }


            //بخش
            if (ViewData["CitiesList"] == null)
            {
                List<CitiesVM> citiesVMList = new List<CitiesVM>();

                try
                {
                    serviceUrl = publicApiUrl + "/api/CitiesManagement/GetAllCitiesListWithOutStrPolygon";

                    GetAllCitiesListPVM getAllCitiesListPVM = new GetAllCitiesListPVM()
                    {
                        //ChildsUsersIds = consoleBusiness.GetDomainChildUserIdsWithStoredProcedure(this.userId.Value,
                        //    this.domainsSettings.DomainSettingId),
                        ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                        this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                    };

                    responseApiCaller = new PublicApiCaller(serviceUrl).GetAllCitiesList(getAllCitiesListPVM);

                    if (responseApiCaller.IsSuccessStatusCode)
                    {
                        jsonResultWithRecordsObjectVM2 = responseApiCaller.JsonResultWithRecordsObjectVM;

                        if (jsonResultWithRecordsObjectVM2 != null)
                        {
                            if (jsonResultWithRecordsObjectVM2.Result.Equals("OK"))
                            {
                                #region Fill UserCreatorName

                                JArray jArray = jsonResultWithRecordsObjectVM2.Records;
                                citiesVMList = jArray.ToObject<List<CitiesVM>>();

                                #endregion
                            }
                        }
                    }
                }
                catch (Exception exc)
                { }

                ViewData["CitiesList"] = citiesVMList;
            }


            //شهر یا منطقه
            if (ViewData["ZonesList"] == null)
            {
                List<ZonesVM> zonesVMList = new List<ZonesVM>();

                try
                {
                    string serviceUrl = publicApiUrl + "/api/ZonesManagement/GetAllZonesListWithOutStrPolygon";

                    GetAllZonesListPVM getAllZonesListPVM = new GetAllZonesListPVM()
                    {
                        ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                        this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                        //ChildsUsersIds = consoleBusiness.GetDomainChildUserIdsWithStoredProcedure(this.userId.Value,
                        //   this.domainsSettings.DomainSettingId),
                    };

                    responseApiCaller = new PublicApiCaller(serviceUrl).GetAllZonesList(getAllZonesListPVM);

                    if (responseApiCaller.IsSuccessStatusCode)
                    {
                        jsonResultWithRecordsObjectVM2 = responseApiCaller.JsonResultWithRecordsObjectVM;

                        if (jsonResultWithRecordsObjectVM2 != null)
                        {
                            if (jsonResultWithRecordsObjectVM2.Result.Equals("OK"))
                            {
                                #region Fill UserCreatorName

                                JArray jArray = jsonResultWithRecordsObjectVM2.Records;
                                zonesVMList = jArray.ToObject<List<ZonesVM>>();

                                #endregion
                            }
                        }
                    }
                }
                catch (Exception exc)
                { }

                ViewData["ZonesList"] = zonesVMList;
            }

            //نوع سند
            if (ViewData["DocumentTypesList"] == null)
            {
                List<DocumentTypesVM> documentTypesVMList = new List<DocumentTypesVM>();

                GetAllDocumentTypesListPVM getAllDocumentTypesListPVM = new GetAllDocumentTypesListPVM()
                {
                    ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                    this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                    //ChildsUsersIds = consoleBusiness.GetDomainChildUserIdsWithStoredProcedure(this.userId.Value,
                    //       this.domainsSettings.DomainSettingId),
                };
                try
                {
                    serviceUrl = teniacoApiUrl + "/api/DocumentTypesManagement/GetAllDocumentTypesList";

                    responseApiCaller = new TeniacoApiCaller(serviceUrl).GetAllDocumentTypesList(getAllDocumentTypesListPVM);

                    if (responseApiCaller.IsSuccessStatusCode)
                    {
                        jsonResultWithRecordsObjectVM2 = responseApiCaller.JsonResultWithRecordsObjectVM;

                        if (jsonResultWithRecordsObjectVM2 != null)
                        {
                            if (jsonResultWithRecordsObjectVM2.Result.Equals("OK"))
                            {
                                #region Fill UserCreatorName

                                JArray jArray = jsonResultWithRecordsObjectVM2.Records;
                                documentTypesVMList = jArray.ToObject<List<DocumentTypesVM>>();


                                #endregion
                            }
                        }
                    }
                }
                catch (Exception exc)
                { }

                ViewData["DocumentTypesList"] = documentTypesVMList;
            }


            //نوع ریشه سند
            if (ViewData["DocumentRootTypesList"] == null)
            {
                List<DocumentRootTypesVM> documentRootTypesVMList = new List<DocumentRootTypesVM>();

                GetAllDocumentRootTypesListPVM getAllDocumentRootTypesListPVM = new GetAllDocumentRootTypesListPVM()
                {
                    ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                    this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                    //ChildsUsersIds = consoleBusiness.GetDomainChildUserIdsWithStoredProcedure(this.userId.Value,
                    //       this.domainsSettings.DomainSettingId),
                };
                try
                {
                    serviceUrl = teniacoApiUrl + "/api/DocumentRootTypesManagement/GetAllDocumentRootTypesList";

                    responseApiCaller = new TeniacoApiCaller(serviceUrl).GetAllDocumentRootTypesList(getAllDocumentRootTypesListPVM);

                    if (responseApiCaller.IsSuccessStatusCode)
                    {
                        jsonResultWithRecordsObjectVM2 = responseApiCaller.JsonResultWithRecordsObjectVM;

                        if (jsonResultWithRecordsObjectVM2 != null)
                        {
                            if (jsonResultWithRecordsObjectVM2.Result.Equals("OK"))
                            {
                                #region Fill UserCreatorName

                                JArray jArray = jsonResultWithRecordsObjectVM2.Records;
                                documentRootTypesVMList = jArray.ToObject<List<DocumentRootTypesVM>>();


                                #endregion
                            }
                        }
                    }
                }
                catch (Exception exc)
                { }

                ViewData["DocumentRootTypesList"] = documentRootTypesVMList;
            }


            //نوع مالکیت 
            if (ViewData["DocumentOwnershipTypesList"] == null)
            {
                List<DocumentOwnershipTypesVM> documentOwnershipTypesVMList = new List<DocumentOwnershipTypesVM>();

                GetAllDocumentOwnershipTypesListPVM getAllDocumentOwnershipTypesListPVM = new GetAllDocumentOwnershipTypesListPVM()
                {
                    ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                    this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                    //ChildsUsersIds = consoleBusiness.GetDomainChildUserIdsWithStoredProcedure(this.userId.Value,
                    //       this.domainsSettings.DomainSettingId),
                };
                try
                {
                    serviceUrl = teniacoApiUrl + "/api/DocumentOwnershipTypesManagement/GetAllDocumentOwnershipTypesList";

                    responseApiCaller = new TeniacoApiCaller(serviceUrl).GetAllDocumentOwnershipTypesList(getAllDocumentOwnershipTypesListPVM);

                    if (responseApiCaller.IsSuccessStatusCode)
                    {
                        jsonResultWithRecordsObjectVM2 = responseApiCaller.JsonResultWithRecordsObjectVM;

                        if (jsonResultWithRecordsObjectVM2 != null)
                        {
                            if (jsonResultWithRecordsObjectVM2.Result.Equals("OK"))
                            {
                                #region Fill UserCreatorName

                                JArray jArray = jsonResultWithRecordsObjectVM2.Records;
                                documentOwnershipTypesVMList = jArray.ToObject<List<DocumentOwnershipTypesVM>>();


                                #endregion
                            }
                        }
                    }
                }
                catch (Exception exc)
                { }

                ViewData["DocumentOwnershipTypesList"] = documentOwnershipTypesVMList;
            }


            //عمر بنا
            if (ViewData["BuildingLifesList"] == null)
            {
                List<BuildingLifesVM> buildingLifesList = new List<BuildingLifesVM>();

                GetAllBuildingLifesListPVM getAllBuildingLifesListPVM = new GetAllBuildingLifesListPVM()
                {
                    ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                    this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                };


                try
                {
                    serviceUrl = melkavanApiUrl + "/api/BuildingLifesManagement/GetAllBuildingLifesList";

                    responseApiCaller = new MelkavanApiCaller(serviceUrl).GetAllBuildingLifesList(getAllBuildingLifesListPVM);

                    if (responseApiCaller.IsSuccessStatusCode)
                    {
                        jsonResultWithRecordsObjectVM2 = responseApiCaller.JsonResultWithRecordsObjectVM;

                        if (jsonResultWithRecordsObjectVM2 != null)
                        {
                            if (jsonResultWithRecordsObjectVM2.Result.Equals("OK"))
                            {
                                #region Fill UserCreatorName

                                JArray jArray = jsonResultWithRecordsObjectVM2.Records;
                                buildingLifesList = jArray.ToObject<List<BuildingLifesVM>>();

                                #endregion
                            }
                        }
                    }
                }
                catch (Exception exc)
                { }

                ViewData["BuildingLifesList"] = buildingLifesList;
            }



            AdvertisementVM advertisementVM = new AdvertisementVM();
            advertisementVM.AdvertisementAddressVM = new AdvertisementAddressVM();

            dynamic expando = new ExpandoObject();
            expando = advertisementVM;





            //List<UserShortCutImagesVM> userShortCutImagesVM = consoleBusiness.GetUserShortCutImagesWithUserId(userId.Value, "fa", "Dashboard");
            //if (ViewData["UserShortCutImages"] == null)
            //    ViewData["UserShortCutImages"] = userShortCutImagesVM;

            //List<LevelShortCutImagesVM> levelShortCutImagesVMList = consoleBusiness.GetLevelShortCutImagesWithLevelId(this.levelId.Value, "fa", "Dashboard");
            ////return View("Index", levelShortCutImagesVMList);

            //return View("Index", levelShortCutImagesVMList);

            //if (this.domainName.Equals("my.teniaco.com"))
            //    return RedirectToAction("Login");

            if ((!this.userId.HasValue || this.userId.Value == 0) && !string.IsNullOrEmpty(Request.Cookies["UserId"]))
            {
                //bool hasError = true;
                try
                {
                    var cookie = Request.Cookies["UserId"];
                    var tempUserId = long.Parse(FrameWork.AES.DecryptStringAES(cookie, key));
                    var user = consoleBusiness.GetUserWithUserId(tempUserId);
                    if (user != null)
                    {

                        if (user.IsActivated.HasValue && user.IsDeleted.HasValue && user.IsActivated.Value && !user.IsDeleted.Value)
                        {
                            var roles = consoleBusiness.GetRolesWithUserId(user.UserId);
                            if (roles.Count > 0)
                            {
                                if (!roles.Any(r => r.RoleName.Equals("Users")))
                                {
                                    //return Json(new
                                    //{
                                    //    Result = "ERROR",
                                    //    Message = "Not Authenticated"
                                    //});
                                }
                                this.userId = user.UserId;
                                roleIds = roles.Select(r => r.RoleId).ToList();
                                userName = user.UserName;
                                var levelsDetails = consoleBusiness.GetLevelsDetailsWithUserIdAndRoleIds(user.UserId, roleIds);
                                levelIds = levelsDetails.Select(l => l.LevelId).ToList();
                                levelNames = string.Join(",", levelsDetails.Select(l => l.LevelName).ToArray());
                                roleIds = roles.Select(r => r.RoleId).ToList();
                                roleNames = string.Join(",", roles.Select(l => l.RoleName).ToArray());

                                var userconfig = consoleBusiness.GetUsersConfigsWithUserId(this.userId.Value);

                                string encryptedUserId = AES.EncryptStringAES(user.UserId.ToString(), key);

                                if (BaseAuthentication.Login(HttpContext,
                                    this.domainsSettings.DomainSettingId.ToString(),
                                    domainAdminId.Value.ToString(),
                                    parentUserId.Value.ToString(),
                                    //user.UserId.ToString(),
                                    encryptedUserId,
                                    roleId.ToString(),
                                    string.Join(",", roleIds.ToArray()),
                                    roleName,
                                    roleNames,
                                    levelId.HasValue ? levelId.Value.ToString() : "",
                                    string.Join(",", levelIds.ToArray()),
                                    levelName,
                                    levelNames,
                                    userName,
                                    name,
                                    family,
                                    personalCode,
                                    email,
                                    userconfig.IsResponsiveList.HasValue ? userconfig.IsResponsiveList.Value.ToString() : ""))
                                {
                                    //DateTime expireTime = DateTime.Now.AddDays(365);
                                    //CookieOptions option = new CookieOptions();
                                    //option.Expires = expireTime;
                                    //Response.Cookies.Append("IsResponsiveList", userconfig.IsResponsiveList.HasValue ?
                                    //userconfig.IsResponsiveList.Value.ToString() : "", option);

                                    //string encryptedUserId = FrameWork.AES.EncryptStringAES(user.UserId.ToString(), key);

                                    //Response.Cookies.Append("UserId", encryptedUserId, option);

                                    //return Json(new
                                    //{
                                    //    Result = "OK",
                                    //    Message = "صبر کنید",
                                    //    ReturnUrl = indexUrl
                                    //});

                                    //hasError = false;

                                }
                                else
                                {
                                    DateTime expireTime = DateTime.Now.AddDays(365);
                                    CookieOptions option = new CookieOptions
                                    {
                                        Expires = expireTime
                                    };
                                    Response.Cookies.Delete("UserId", option);
                                    this.userId = 0;
                                }
                            }
                        }
                    }

                }
                catch
                {
                    //hasError = true;
                    DateTime expireTime = DateTime.Now.AddDays(365);
                    CookieOptions option = new CookieOptions
                    {
                        Expires = expireTime
                    };
                    Response.Cookies.Delete("UserId", option);
                    this.userId = 0;
                }

            }

            if (ViewData["BannersList"] == null)
            {
                List<BannersVM> bannerVMs = new List<BannersVM>();

                try
                {
                    JsonResultWithRecordsObjectVM jsonResultWithRecordsObjectVM = new JsonResultWithRecordsObjectVM(new object() { });


                    string ServiceUrl = melkavanApiUrl + "/api/BannersManagement/GetAllBannersList";

                    GetAllBannersListPVM getAllBannersListPVM =
                        new GetAllBannersListPVM()
                        {
                            ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                            this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                            // ChildsUsersIds = consoleBusiness.GetDomainChildUserIdsWithStoredProcedure(this.userId.Value,
                            //this.domainsSettings.DomainSettingId),
                        };
                    responseApiCaller = new MelkavanApiCaller(ServiceUrl).GetAllBannersList(getAllBannersListPVM);

                    if (responseApiCaller.IsSuccessStatusCode)
                    {
                        jsonResultWithRecordsObjectVM = responseApiCaller.JsonResultWithRecordsObjectVM;

                        if (jsonResultWithRecordsObjectVM != null)
                        {
                            if (jsonResultWithRecordsObjectVM.Result.Equals("OK"))
                            {

                                JArray jArray = jsonResultWithRecordsObjectVM.Records;
                                var records = jArray.ToObject<List<BannersVM>>();

                                bannerVMs = records;
                                //personsVM.Email = user.Email;
                            }
                        }
                    }
                }
                catch (Exception exc)
                { }
                ViewData["BannersList"] = bannerVMs;
            }

            if (ViewData["UserDevicePlatform"] == null)
            {
                ViewData["UserDevicePlatform"] = FrameWork.PlatformInfo.IsMobile(Request.Headers["User-Agent"].ToString()).Equals(true) ? "mobile" : "desktop";
            }
            var onlyFavorite = Request.Cookies["OnlyFavorite"];
            if (!string.IsNullOrEmpty(onlyFavorite) && this.userId.HasValue && this.userId.Value != 0)
            {
                ViewData["OnlyFavorite"] = onlyFavorite == "True";
            }
            //  ViewData["DomainName"] = this.domainsSettings.DomainName;
            ViewData["DomainName"] = "melkavan.com";

            return View("PublicIndex");
        }

        //لیست آگهی ها
        [AllowAnonymous]
        [HttpPost]
        [AjaxOnly]
        public IActionResult GetListOfAdverisementsAdvanceSearch(
            int startIndex,
            int pageSize,
            bool? onlyFavorite = false,
            string searchString = null, bool IsFiltered = false,
             //parameters for advanced filters
             int? AdvertisementTypeId = null,
             int? PropertyTypeId = null,
             string? TypeOfUseId = null,
             long? StateId = null,
             long? CityId = null,
             long? ZoneId = null,
             string? TownName = null,
             string? FromArea = null,
             string? ToArea = null,
             int? FromFoundation = null,
             int? ToFoundation = null,
             long? FromPrice = null,
             long? ToPrice = null,
             string? BuildingLifeId = null,
             int? FromRebuiltInYear = null,
             int? ToRebuiltInYear = null,
             int? DocumentTypeId = null,
             int? DocumentRootTypeId = null,
             int? DocumentOwnershipTypeId = null,
             long? DepositFromPrice = null,
             long? DepositToPrice = null,
             long? RentFromPrice = null,
             long? RentToPrice = null,
             int? MaritalStatusId = null,
             string? Convertable = null,
             Dictionary<string, string>? Features = null)
        {
            List<AdvertisementAdvanceSearchVM> advertisementAdvanceSearchVMs = new List<AdvertisementAdvanceSearchVM>();

            try
            {
                JsonResultWithRecordsObjectVM jsonResultWithRecordsObjectVM = new JsonResultWithRecordsObjectVM(new object() { });

                //for fixing auto binding bug when no features is selected
                if (Features.TryGetValue("startIndex", out string dummy))
                {
                    Features = null;
                }



                string ServiceUrl = melkavanApiUrl + "/api/AdvertisementManagement/GetListOfAdverisementsAdvanceSearch";

                GetListOfAdvertisementAdvanceSearchPVM getListOfAdvertisementAdvanceSearchPVM =
                    new GetListOfAdvertisementAdvanceSearchPVM()
                    {
                        ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                            this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                        jtPageSize = pageSize,
                        jtStartIndex = startIndex,
                        UserId = this.userId.Value,
                        OnlyFavorite = onlyFavorite,
                        AdvertisementTitle = searchString,
                        IsFiltered = IsFiltered,
                        //Advanced search
                        AdvertisementTypeId = AdvertisementTypeId,
                        PropertyTypeId = PropertyTypeId,
                        TypeOfUseId = TypeOfUseId,
                        StateId = StateId,
                        CityId = CityId,
                        ZoneId = ZoneId,
                        TownName = TownName,
                        FromArea = FromArea,
                        ToArea = ToArea,
                        FromFoundation = FromFoundation,
                        ToFoundation = ToFoundation,
                        FromPrice = FromPrice,
                        ToPrice = ToPrice,
                        BuildingLifeId = BuildingLifeId,
                        FromRebuiltInYear = FromRebuiltInYear,
                        ToRebuiltInYear = ToRebuiltInYear,
                        DocumentTypeId = DocumentTypeId,
                        DocumentRootTypeId = DocumentRootTypeId,
                        DocumentOwnershipTypeId = DocumentOwnershipTypeId,
                        DepositFromPrice = DepositFromPrice,
                        DepositToPrice = DepositToPrice,
                        RentFromPrice = RentFromPrice,
                        RentToPrice = RentToPrice,
                        MaritalStatusId = MaritalStatusId,
                        Convertable = Convertable,
                        Features = Features,
                    };





                responseApiCaller = new MelkavanApiCaller(ServiceUrl).GetListOfAdverisementsAdvanceSearch(getListOfAdvertisementAdvanceSearchPVM);

                if (responseApiCaller.IsSuccessStatusCode)
                {

                    jsonResultWithRecordsObjectVM = responseApiCaller.JsonResultWithRecordsObjectVM;

                    if (jsonResultWithRecordsObjectVM != null)
                    {

                        if (jsonResultWithRecordsObjectVM.Result.Equals("OK"))
                        {

                            JArray jArray = jsonResultWithRecordsObjectVM.Records;
                            var records = jArray.ToObject<List<AdvertisementAdvanceSearchVM>>();

                            return Json(new
                            {
                                Result = "OK",
                                Records = records,
                                Message = "Success"
                            });
                        }
                    }
                }
            }
            catch (Exception exc)
            { }

            return Json(new
            {
                Result = "ERROR",
                Message = "خطا"
            });


        }



        #region OldCode for AdvertisementsList


        //لیست آگهی ها
        [AllowAnonymous]
        [HttpPost]
        [AjaxOnly]
        public IActionResult GetAdvertisementsList1(int startIndex, int pageSize, bool? onlyFavorite = false, string searchString = null)
        {
            List<AdvertisementVM> advertisementVMs = new List<AdvertisementVM>();
            //var user = consoleBusiness.GetUserWithUserId(this.userId.Value);

            //string str = "1- ";

            try
            {
                JsonResultWithRecordsObjectVM jsonResultWithRecordsObjectVM = new JsonResultWithRecordsObjectVM(new object() { });

                var a = consoleBusiness.GetRoleIdsWithUserId(this.domainAdminId.Value);
                var b = consoleBusiness.GetLevelIdsWithUserId(this.domainAdminId.Value);


                string ServiceUrl = melkavanApiUrl + "/api/AdvertisementManagement/GetListOfAdvertisement";

                GetListOfAdvertisementPVM getListOfAdvertisementPVM =
                    new GetListOfAdvertisementPVM()
                    {
                        //ChildsUsersIds = consoleBusiness.GetDomainChildUserIdsWithStoredProcedure(this.userId.Value,
                        //   this.domainsSettings.DomainSettingId),
                        //ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, !this.parentUserId.Value.Equals(0) ?this.parentUserId.Value : this.domainAdminId.Value,
                        // this.domainsSettings.UserIdCreator.Value, !this.roleIds.Count.Equals(0)? this.roleIds : consoleBusiness.GetRoleIdsWithUserId(this.domainAdminId.Value), !this.levelIds.Count.Equals(0) ? this.levelIds: consoleBusiness.GetLevelIdsWithUserId(this.domainAdminId.Value)),
                        //OwnerPersonId = user.UserId,
                        ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                            this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                        jtPageSize = pageSize,
                        jtStartIndex = startIndex,
                        UserId = this.userId.Value,
                        OnlyFavorite = onlyFavorite,
                        AdvertisementTitle = searchString,
                    };

                //str += "2- " + ServiceUrl;

                responseApiCaller = new MelkavanApiCaller(ServiceUrl).GetListOfAdvertisement(getListOfAdvertisementPVM);
                //str += "3- " + JsonConvert.SerializeObject(getListOfAdvertisementPVM);
                if (responseApiCaller.IsSuccessStatusCode)
                {
                    //str += "4- ";
                    jsonResultWithRecordsObjectVM = responseApiCaller.JsonResultWithRecordsObjectVM;
                    //str += "5- ";
                    if (jsonResultWithRecordsObjectVM != null)
                    {
                        //str += "6- ";
                        if (jsonResultWithRecordsObjectVM.Result.Equals("OK"))
                        {
                            //str += "7- ";
                            JArray jArray = jsonResultWithRecordsObjectVM.Records;
                            var records = jArray.ToObject<List<AdvertisementVM>>();

                            //str += "8- ";

                            return Json(new
                            {
                                Result = "OK",
                                Records = records,
                                Message = "Success"
                            });
                        }
                    }
                }
            }
            catch (Exception exc)
            { }
            return Json(new
            {
                Result = "ERROR",
                //Message = "خطا" + str,
                Message = "خطا"// + str,
            });


        }

        #endregion

        #endregion

        #region Melkavan Login




        [HttpPost]
        [AjaxOnly]
        [AllowAnonymous]
        public IActionResult MelkavanLogin(MelkavanLoginVM loginVM)
        {
            JsonResultWithRecordObjectVM jsonResultWithRecordObjectVM = new JsonResultWithRecordObjectVM(new object() { });

            if (string.IsNullOrEmpty(loginVM.UserName))
            {
                return Json(new
                {
                    Result = "ERROR",
                    Message = "NumberIsEmpty"
                });
            }

            if ((loginVM.UserName.Length < 11) || (!loginVM.UserName.StartsWith("09")))
            {
                return Json(new
                {
                    Result = "ERROR",
                    Message = "NumbersIsNotCorrect"
                });
            }

            try
            {

                string serviceUrl = melkavanApiUrl + "/api/MelkavanLoginManagement/SendSMSservice";

                MelkavanLoginPVM melkavanLoginPVM = new MelkavanLoginPVM()
                {
                    MelkavanLoginVM = loginVM,

                };

                responseApiCaller = new MelkavanApiCaller(serviceUrl).SendSMSservice(melkavanLoginPVM);

                if (responseApiCaller.IsSuccessStatusCode)
                {
                    jsonResultWithRecordObjectVM = responseApiCaller.JsonResultWithRecordObjectVM;

                    if (jsonResultWithRecordObjectVM != null)
                    {
                        if (jsonResultWithRecordObjectVM.Result.Equals("OK"))
                        {

                            var TempVerifyCode = jsonResultWithRecordObjectVM.Record;

                            if (TempVerifyCode != null)
                            {



                            }

                        }
                        else
                            if (jsonResultWithRecordObjectVM.Result.Equals("ERROR") &&
                            jsonResultWithRecordObjectVM.Message.Equals("LoginErrorMessage"))
                        {
                            return Json(new
                            {
                                Result = "ERROR",
                                Message = "SMSNotWorking"
                            });
                        }
                    }

                }
            }
            catch (Exception ex)
            { }

            return Json(new
            {
                Result = "OK",
                userName = jsonResultWithRecordObjectVM.Value,
                verify = jsonResultWithRecordObjectVM.Record
            });
        }

        #region ConvertToHash
        [NonAction]
        public static string ComputeSha256Hash(string input)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Convert the input string to a byte array and compute the hash.
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Convert the byte array to a hexadecimal string.
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
        #endregion
        #region Mine



        //[AjaxOnly]
        //[HttpPost]
        //[AllowAnonymous]
        //public IActionResult MelkavanVerify(MelkavanLoginVM loginVM)
        //{
        //    JsonResultWithRecordObjectVM jsonResultWithRecordObjectVM = new JsonResultWithRecordObjectVM(new object() { });

        //    bool? showCaptcha = false;
        //    try
        //    {
        //        if (loginVM != null)
        //        {

        //            ModelState.Remove("DomainSettingId");
        //            ModelState.Remove("ParentUserId");
        //            ModelState.Remove("Password");
        //            ModelState.Remove("UserName");



        //            ModelState.Remove("CustomUsersVM.UserName");
        //            ModelState.Remove("CustomUsersVM.NameFamilyOfUserCreator");
        //            ModelState.Remove("CustomUsersVM.Email");
        //            ModelState.Remove("CustomUsersVM.UserId");
        //            ModelState.Remove("CustomUsersVM.RoleIds");
        //            ModelState.Remove("CustomUsersVM.LevelIds");
        //            ModelState.Remove("CustomUsersVM.ReplyPassword");
        //            ModelState.Remove("CustomUsersVM.Password");
        //            ModelState.Remove("CustomUsersVM.ParentUserId");
        //            ModelState.Remove("CustomUsersVM.Name");
        //            ModelState.Remove("CustomUsersVM.Family");
        //            ModelState.Remove("CustomUsersVM.Mobile");

        //            if (ModelState.IsValid)
        //            {
        //                if (HttpContext.Session.IsAvailable)
        //                {
        //                    if (HttpContext.Session.Keys.Any())
        //                    {
        //                        if (HttpContext.Session.GetString("CaptchaCode") != Request.Form["txtCaptchaCode"])
        //                        {
        //                            return Json(new
        //                            {
        //                                Result = "ERROR",
        //                                Message = "ErrorCaptchaCode"
        //                            });
        //                        }
        //                    }
        //                }

        //                string ip = this.Request.HttpContext.Connection.RemoteIpAddress.ToString();
        //                var count = publicServicesBusiness.GetCountOfUserRequestLogsWithUserIpAndAddressAndDate(ip,
        //                    DateTime.Now,
        //                    5);

        //                if (count > domainsSettings.CountOfEnterWrongPasswordForUserLocked)
        //                {
        //                    consoleBusiness.DeactivateUser(loginVM.UserName);
        //                    return Json(new
        //                    {
        //                        Result = "OK",
        //                        Message = "Waiting",
        //                        ReturnUrl = "/Home/LockedUser"
        //                    });

        //                }

        //                if (count > domainsSettings.CountOfEnterWrongPassword)
        //                {
        //                    showCaptcha = true;
        //                    ViewData["ShowCaptcha"] = true;
        //                }

        //                //اگر کدی که کاربر وارد کرد برابر با کدی که برایش ارسال شده است باشد
        //                // یک کاربر باید در سیستم اضافه شود
        //                if (ComputeSha256Hash(loginVM.Password).Equals(loginVM.CaptchaCode))
        //                {
        //                    string serviceUrl = melkavanApiUrl + "/api/MelkavanLoginManagement/MelkavanLogin";

        //                    MelkavanLoginPVM melkavanLoginPVM = new MelkavanLoginPVM()
        //                    {
        //                        MelkavanLoginVM = loginVM,
        //                    };

        //                    responseApiCaller = new MelkavanApiCaller(serviceUrl).MelkavanLogin(melkavanLoginPVM);

        //                    if (responseApiCaller.IsSuccessStatusCode)
        //                    {
        //                        jsonResultWithRecordObjectVM = responseApiCaller.JsonResultWithRecordObjectVM;

        //                        if (jsonResultWithRecordObjectVM != null)
        //                        {
        //                            if (jsonResultWithRecordObjectVM.Result.Equals("OK"))
        //                            {

        //                                JObject jObject = jsonResultWithRecordObjectVM.Record;
        //                                var record = jObject.ToObject<CustomUsersVM>();

        //                                if (record != null)
        //                                {
        //                                    if (record.UserId > 0)
        //                                    {
        //                                        userId = record.UserId;

        //                                        #region create user folders

        //                                        var domainSettings = consoleBusiness.GetDomainsSettingsWithDomainSettingId(record.DomainSettingId, this.domainsSettings.UserIdCreator.Value);

        //                                        string userFolder = hostEnvironment.ContentRootPath + "\\wwwroot\\Files\\UserFiles\\";

        //                                        if (!System.IO.Directory.Exists(userFolder + "\\" + domainSettings.DomainName + "\\" + record.UserId))
        //                                        {
        //                                            System.IO.Directory.CreateDirectory(userFolder + "\\" + domainSettings.DomainName + "\\" + record.UserId);
        //                                            System.Threading.Thread.Sleep(100);
        //                                        }

        //                                        if (!System.IO.Directory.Exists(userFolder + "\\" + domainSettings.DomainName + "\\" + record.UserId + "\\UserImages"))
        //                                        {
        //                                            System.IO.Directory.CreateDirectory(userFolder + "\\" + domainSettings.DomainName + "\\" + record.UserId + "\\UserImages");
        //                                            System.Threading.Thread.Sleep(100);
        //                                        }

        //                                        #endregion

        //                                        #region if CreateSubDomainPerNewUser config for user domain is activated then create sub domain per user

        //                                        if (domainSettings.CreateSubDomainPerNewUser)
        //                                        {
        //                                            try
        //                                            {
        //                                                string siteName = record.UserName + "." + domainSettings.DomainName;
        //                                                string ipAddress = "";
        //                                                string tcpPort = "";
        //                                                string AdminHeader = "";
        //                                                string protocol = "";

        //                                                if (string.IsNullOrEmpty(siteName))
        //                                                {

        //                                                }
        //                                                //get the server manager instance

        //                                                using (ServerManager mgr = ServerManager.OpenRemote(@"\\Qasql01\c$\Windows\System32\inetsrv\config\applicationAdmin.config"))
        //                                                //using (ServerManager mgr = new ServerManager())
        //                                                {
        //                                                    SiteCollection sites = mgr.Sites;
        //                                                    Site site = mgr.Sites[siteName];
        //                                                    if (site != null)
        //                                                    {
        //                                                        string bind = ipAddress + ":" + tcpPort + ":" + AdminHeader;
        //                                                        //check the binding exists or not
        //                                                        foreach (Binding b in site.Bindings)
        //                                                        {
        //                                                            if (b.Protocol == protocol && b.BindingInformation == bind)
        //                                                            {
        //                                                                throw new Exception("A binding with the same ip, port and Admin header already exists.");
        //                                                            }
        //                                                        }
        //                                                        Binding newBinding = site.Bindings.CreateElement();
        //                                                        newBinding.Protocol = protocol;
        //                                                        newBinding.BindingInformation = bind;
        //                                                        site.Bindings.Add(newBinding);
        //                                                        mgr.CommitChanges();
        //                                                    }
        //                                                    else
        //                                                    { }
        //                                                }
        //                                            }
        //                                            catch (Exception ex)
        //                                            {

        //                                            }

        //                                        }
        //                                        else
        //                                        {
        //                                            return Json(new
        //                                            {
        //                                                Result = "ERROR",
        //                                                Message = "domainSettingsIsNull"
        //                                            });
        //                                        }

        //                                        #endregion

        //                                        #region Set Cookie


        //                                        try
        //                                        {
        //                                            var indexUrl = jsonResultWithRecordObjectVM.ContentType;

        //                                            var userconfig = consoleBusiness.GetUsersConfigsWithUserId(userId.Value);
        //                                            string encryptedUserId = "";
        //                                            encryptedUserId = AES.EncryptStringAES(userId.ToString(), key);

        //                                            if (BaseAuthentication.Login(HttpContext,
        //                                                this.domainsSettings.DomainSettingId.ToString(),
        //                                                domainAdminId.Value.ToString(),
        //                                                parentUserId.Value.ToString(),
        //                                                encryptedUserId,
        //                                                roleId.ToString(),
        //                                                string.Join(",", roleIds.ToArray()),
        //                                                roleName,
        //                                                roleNames,
        //                                                levelId.HasValue ? levelId.Value.ToString() : "",
        //                                                string.Join(",", levelIds.ToArray()),
        //                                                levelName,
        //                                                levelNames,
        //                                                userName,
        //                                                name,
        //                                                family,
        //                                                personalCode,
        //                                                email,
        //                                                userconfig.IsResponsiveList.HasValue ? userconfig.IsResponsiveList.Value.ToString() : ""))
        //                                            {
        //                                                DateTime expireTime = DateTime.Now.AddDays(365);
        //                                                CookieOptions option = new CookieOptions();
        //                                                option.Expires = expireTime;
        //                                                Response.Cookies.Append("IsResponsiveList", userconfig.IsResponsiveList.HasValue ?
        //                                                userconfig.IsResponsiveList.Value.ToString() : "", option);

        //                                                Response.Cookies.Append("UserId", encryptedUserId, option);

        //                                                return Json(new
        //                                                {
        //                                                    Result = "OK",
        //                                                    Message = "صبر کنید",
        //                                                    userName = loginVM.UserName,
        //                                                    ReturnUrl = indexUrl
        //                                                });


        //                                            }
        //                                            else
        //                                            {
        //                                                return Json(new
        //                                                {
        //                                                    Result = "ERROR",
        //                                                    Message = "DisabledUserNameMessage",
        //                                                    ShowCaptcha = (showCaptcha.Value == true) ? showCaptcha.Value : (bool?)null
        //                                                });
        //                                            }


        //                                        }
        //                                        catch (Exception exc)
        //                                        {
        //                                            return Json(new
        //                                            {
        //                                                Result = "ERROR",
        //                                                Message = "CookieIsNotSet",
        //                                                Text = exc.InnerException.Message,
        //                                            });
        //                                        }
        //                                        #endregion
        //                                    }
        //                                    else
        //                                    {
        //                                        return Json(new
        //                                        {
        //                                            Result = "ERROR",
        //                                            Message = "UserIdIsNull"
        //                                        });
        //                                    }
        //                                }

        //                            }
        //                            else
        //                                if (jsonResultWithRecordObjectVM.Result.Equals("ERROR") &&
        //                                jsonResultWithRecordObjectVM.Message.Equals("LoginErrorMessage"))
        //                            {
        //                                return Json(new
        //                                {
        //                                    Result = "ERROR",
        //                                    Message = "LoginFailed"
        //                                });
        //                            }
        //                        }
        //                        else
        //                        {
        //                            return Json(new
        //                            {
        //                                Result = "ERROR",
        //                                Message = "ResultIsNull"
        //                            });
        //                        }

        //                    }
        //                    else
        //                    {
        //                        return Json(new
        //                        {
        //                            Result = "ERROR",
        //                            Message = "responseApiCallerIsNull"
        //                        });
        //                    }
        //                }
        //                else
        //                {
        //                    return Json(new
        //                    {
        //                        Result = "ERROR",
        //                        Message = "HashIsNotWorking"

        //                    });
        //                }
        //            }
        //            else
        //            {
        //                // استخراج خطاهای ModelState به فرمت JSON
        //                var errors = ModelState
        //                    .Where(ms => ms.Value.Errors.Any())
        //                    .ToDictionary(
        //                        kvp => kvp.Key,
        //                        kvp => kvp.Value.Errors
        //                                 .Select(e => e.ErrorMessage)
        //                                 .ToArray()
        //                    );

        //                return Json(new
        //                {
        //                    Result = "ERROR",
        //                    Message = "ModelStateIsNotValid",
        //                    Errors = errors
        //                });
        //            }
        //        }
        //        else
        //        {
        //            return Json(new
        //            {
        //                Result = "ERROR",
        //                Message = "LoginVMIsNull",

        //            });
        //        }



        //    }


        //    catch (Exception exc)
        //    { }

        //    return Json(new
        //    {
        //        Result = "ERROR",
        //        Message = "RandomError",
        //        Text = loginVM.ToString()

        //    });
        //}

        #endregion



        [AjaxOnly]
        [HttpPost]
        [AllowAnonymous]
        public IActionResult MelkavanVerify(MelkavanLoginVM loginVM)
        {
            JsonResultWithRecordObjectVM jsonResultWithRecordObjectVM = new JsonResultWithRecordObjectVM(new object() { });

            bool? showCaptcha = false;
            try
            {

                if (ModelState.IsValid)
                {
                    if (HttpContext.Session.IsAvailable)
                    {
                        if (HttpContext.Session.Keys.Any())
                        {
                            if (HttpContext.Session.GetString("CaptchaCode") != Request.Form["txtCaptchaCode"])
                            {
                                return Json(new
                                {
                                    Result = "ERROR",
                                    Message = "ErrorCaptchaCode"
                                });
                            }
                        }
                    }

                    string ip = this.Request.HttpContext.Connection.RemoteIpAddress.ToString();
                    var count = publicServicesBusiness.GetCountOfUserRequestLogsWithUserIpAndAddressAndDate(ip,
                        DateTime.Now,
                        5);

                    if (count > domainsSettings.CountOfEnterWrongPasswordForUserLocked)
                    {
                        consoleBusiness.DeactivateUser(loginVM.UserName);
                        return Json(new
                        {
                            Result = "OK",
                            Message = "Waiting",
                            ReturnUrl = "/Home/LockedUser"
                        });

                    }

                    if (count > domainsSettings.CountOfEnterWrongPassword)
                    {
                        showCaptcha = true;
                        ViewData["ShowCaptcha"] = true;
                    }

                    //اگر کدی که کاربر وارد کرد برابر با کدی که برایش ارسال شده است باشد
                    // یک کاربر باید در سیستم اضافه شود
                    if (ComputeSha256Hash(loginVM.Password).Equals(loginVM.CaptchaCode))
                    {
                        string serviceUrl = melkavanApiUrl + "/api/MelkavanLoginManagement/MelkavanLogin";

                        MelkavanLoginPVM melkavanLoginPVM = new MelkavanLoginPVM()
                        {
                            MelkavanLoginVM = loginVM,
                        };

                        responseApiCaller = new MelkavanApiCaller(serviceUrl).MelkavanLogin(melkavanLoginPVM);

                        if (responseApiCaller.IsSuccessStatusCode)
                        {
                            jsonResultWithRecordObjectVM = responseApiCaller.JsonResultWithRecordObjectVM;

                            if (jsonResultWithRecordObjectVM != null)
                            {
                                if (jsonResultWithRecordObjectVM.Result.Equals("OK"))
                                {

                                    JObject jObject = jsonResultWithRecordObjectVM.Record;
                                    var record = jObject.ToObject<CustomUsersVM>();

                                    if (record != null)
                                    {
                                        if (record.UserId > 0)
                                        {
                                            userId = record.UserId;

                                            #region create user folders

                                            //var domainSettings = consoleBusiness.GetDomainsSettingsWithDomainSettingId(record.DomainSettingId, this.domainsSettings.UserIdCreator.Value);

                                            var domainSettings = consoleBusiness.CmsDb.DomainsSettings.Where(c => c.DomainSettingId.Equals(record.DomainSettingId)).FirstOrDefault();

                                            string userFolder = hostEnvironment.ContentRootPath + "\\wwwroot\\Files\\UserFiles\\";


                                            if (!System.IO.Directory.Exists(userFolder + "\\" + domainSettings.DomainName + "\\" + record.UserId))
                                            {
                                                System.IO.Directory.CreateDirectory(userFolder + "\\" + domainSettings.DomainName + "\\" + record.UserId);
                                                System.Threading.Thread.Sleep(100);
                                            }

                                            if (!System.IO.Directory.Exists(userFolder + "\\" + domainSettings.DomainName + "\\" + record.UserId + "\\UserImages"))
                                            {
                                                System.IO.Directory.CreateDirectory(userFolder + "\\" + domainSettings.DomainName + "\\" + record.UserId + "\\UserImages");
                                                System.Threading.Thread.Sleep(100);
                                            }


                                            #endregion

                                            #region if CreateSubDomainPerNewUser config for user domain is activated then create sub domain per user

                                            //if (domainSettings.CreateSubDomainPerNewUser)
                                            //{
                                            //    try
                                            //    {
                                            //        string siteName = record.UserName + "." + domainSettings.DomainName;
                                            //        string ipAddress = "";
                                            //        string tcpPort = "";
                                            //        string AdminHeader = "";
                                            //        string protocol = "";

                                            //        if (string.IsNullOrEmpty(siteName))
                                            //        {

                                            //        }
                                            //        //get the server manager instance

                                            //        using (ServerManager mgr = ServerManager.OpenRemote(@"\\Qasql01\c$\Windows\System32\inetsrv\config\applicationAdmin.config"))
                                            //        //using (ServerManager mgr = new ServerManager())
                                            //        {
                                            //            SiteCollection sites = mgr.Sites;
                                            //            Site site = mgr.Sites[siteName];
                                            //            if (site != null)
                                            //            {
                                            //                string bind = ipAddress + ":" + tcpPort + ":" + AdminHeader;
                                            //                //check the binding exists or not
                                            //                foreach (Binding b in site.Bindings)
                                            //                {
                                            //                    if (b.Protocol == protocol && b.BindingInformation == bind)
                                            //                    {
                                            //                        throw new Exception("A binding with the same ip, port and Admin header already exists.");
                                            //                    }
                                            //                }
                                            //                Binding newBinding = site.Bindings.CreateElement();
                                            //                newBinding.Protocol = protocol;
                                            //                newBinding.BindingInformation = bind;
                                            //                site.Bindings.Add(newBinding);
                                            //                mgr.CommitChanges();
                                            //            }
                                            //            else
                                            //            { }
                                            //        }
                                            //    }
                                            //    catch (Exception ex)
                                            //    {
                                            //        return Json(new
                                            //        {
                                            //            Result = "ERROR",
                                            //            Message = "Exception",
                                            //            Exception = ex
                                            //        });
                                            //    }

                                            //}

                                            #endregion

                                            #region Set Cookie


                                            try
                                            {
                                                var indexUrl = jsonResultWithRecordObjectVM.ContentType;

                                                Console.WriteLine(indexUrl);

                                                var userconfig = consoleBusiness.GetUsersConfigsWithUserId(userId.Value);
                                                string encryptedUserId = "";
                                                encryptedUserId = AES.EncryptStringAES(userId.ToString(), key);

                                                if (BaseAuthentication.Login(HttpContext,
                                                    this.domainsSettings.DomainSettingId.ToString(),
                                                    domainAdminId.Value.ToString(),
                                                    parentUserId.Value.ToString(),
                                                    encryptedUserId,
                                                    roleId.ToString(),
                                                    string.Join(",", roleIds.ToArray()),
                                                    roleName,
                                                    roleNames,
                                                    levelId.HasValue ? levelId.Value.ToString() : "",
                                                    string.Join(",", levelIds.ToArray()),
                                                    levelName,
                                                    levelNames,
                                                    userName,
                                                    name,
                                                    family,
                                                    personalCode,
                                                    email,
                                                    userconfig.IsResponsiveList.HasValue ? userconfig.IsResponsiveList.Value.ToString() : ""))
                                                {
                                                    DateTime expireTime = DateTime.Now.AddDays(365);
                                                    CookieOptions option = new CookieOptions();
                                                    option.Expires = expireTime;
                                                    Response.Cookies.Append("IsResponsiveList", userconfig.IsResponsiveList.HasValue ?
                                                    userconfig.IsResponsiveList.Value.ToString() : "", option);

                                                    Response.Cookies.Append("UserId", encryptedUserId, option);

                                                    return Json(new
                                                    {
                                                        Result = "OK",
                                                        Message = "صبر کنید",
                                                        userName = loginVM.UserName,
                                                        ReturnUrl = indexUrl
                                                    });


                                                }
                                                else
                                                {
                                                    return Json(new
                                                    {
                                                        Result = "ERROR",
                                                        Message = "DisabledUserNameMessage",
                                                        ShowCaptcha = (showCaptcha.Value == true) ? showCaptcha.Value : (bool?)null
                                                    });
                                                }


                                            }
                                            catch (Exception exc)
                                            {
                                                return Json(new
                                                {
                                                    Result = "ERROR",
                                                    Message = "ProblemWithCookie",
                                                    CookieException = exc.ToString()
                                                });
                                            }
                                            #endregion
                                        }
                                        else
                                        {
                                            return Json(new
                                            {
                                                Result = "ERROR",
                                                Message = "UserIdIsNull"

                                            });
                                        }
                                    }
                                    else
                                    {
                                        return Json(new
                                        {
                                            Result = "ERROR",
                                            Message = "RecordIsNull"
                                        });
                                    }

                                }
                                else
                                    if (jsonResultWithRecordObjectVM.Result.Equals("ERROR") &&
                                    jsonResultWithRecordObjectVM.Message.Equals("LoginErrorMessage"))
                                {
                                    return Json(new
                                    {
                                        Result = "ERROR",
                                        Message = "CallerIsNotWorking"
                                    });
                                }
                            }
                            else
                            {
                                return Json(new
                                {
                                    Result = "ERROR",
                                    Message = "jsonResultWithRecordObjectVMIsNull",
                                    JsonResultWithRecordObjectVM = jsonResultWithRecordObjectVM
                                });
                            }

                        }
                        else
                        {
                            return Json(new
                            {
                                Result = "ERROR",
                                Message = "responseApiCallerNotWorking",
                                ResponseApiCaller = responseApiCaller
                            });
                        }
                    }
                    else
                    {
                        return Json(new
                        {
                            Result = "ERROR",
                            Message = "HashNotWorking"
                        });
                    }
                }
                else
                {
                    return Json(new
                    {
                        Result = "ERROR",
                        Message = "ModelStateIsNotValid"
                    });
                }
            }


            catch (Exception exc)
            {
            }

            return Json(new
            {
                Result = "ERROR",
                Message = "RandomError"
            });
        }




        [AllowAnonymous]
        public ActionResult LogOffMelkavan()
        {
            DateTime expireTime = DateTime.Now.AddDays(365);
            CookieOptions option = new CookieOptions();
            option.Expires = expireTime;
            Response.Cookies.Delete("UserId", option);

            if (BaseAuthentication.LogOff(HttpContext))
                return RedirectToAction("Index", "MelkavanPanel");

            return RedirectToAction("Index", "MelkavanPanel");
        }


        #endregion

        #region SendSmsService

        //public class ConfirmCodeViewModel
        //{
        //    public string UserName { get; set; }
        //    public string VirificationCode { get; set; }
        //    public bool Result { get; set; }
        //}

        //public class SendSmsService
        //{
        //    public string RandomCode()
        //    {
        //        Random rd = new Random();
        //        int randomNumber = rd.Next(1000, 9999);
        //        return randomNumber.ToString();
        //    }
        //    public string SendSms(string phoneNumber, string verifyCode, SmsSendersVM smsSendersVM)
        //    {
        //        if (!string.IsNullOrWhiteSpace(phoneNumber))
        //        {
        //            var request = (HttpWebRequest)WebRequest.Create("http://sms.safironline.ir/webservice/rest/sms_send?");

        //            var MobileNumber = phoneNumber;
        //            var postData = "note_arr= کد تایید کاربر ملکاوان : " + verifyCode + "&login_username=" + smsSendersVM.UserName + "& login_password=" + smsSendersVM.Password + "&receiver_number=" + MobileNumber + "&sender_number=" + smsSendersVM.SmsSenderNumber;
        //            //var postData = "login_username=teniaco&login_password=Avoi2@1400&receiver_number=09122060766&note_arr[]=سلام&sender_number=500021163";

        //            var data = Encoding.UTF8.GetBytes(postData);

        //            request.Method = "POST";
        //            request.ContentType = "application/x-www-form-urlencoded";
        //            request.ContentLength = data.Length;

        //            using (var stream = request.GetRequestStream())
        //            {
        //                stream.Write(data, 0, data.Length);
        //            }

        //            var response = (HttpWebResponse)request.GetResponse();

        //            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

        //            var MsgStat = GetEnumFromJsonString(responseString);
        //            return MsgStat.ToString();
        //        }
        //        else
        //        {
        //            return "لطفا شماره موبایل را وارد کنید";
        //        }
        //    }
        //    public static Enum GetEnumFromJsonString(String jsonStr)
        //    {
        //        return ConvertStatusCodeToEnum(GetStatusCode(GetListStrJson(jsonStr)));
        //    }

        //    private static String GetListStrJson(String JsonInpVal)
        //    {
        //        try
        //        {
        //            if (JsonConvert.DeserializeObject<JsonModel>(JsonInpVal).list[0] == null)
        //                return JsonConvert.DeserializeObject<JsonModel>(JsonInpVal).result.ToString();
        //            else
        //                return JsonConvert.DeserializeObject<JsonModel>(JsonInpVal).list[0];

        //        }
        //        catch (Exception ex)
        //        {
        //            //Save exception log:
        //            throw new Exception(ex.Message);
        //        }
        //    }

        //    private static int GetStatusCode(String listInput)
        //    {
        //        var StrArray = listInput.Split('_');
        //        return int.Parse(StrArray[StrArray.Length - 1]);
        //    }

        //    private static Enum ConvertStatusCodeToEnum(int inpCodeStatus)
        //    {
        //        return (Conditions)inpCodeStatus;
        //    }
        //}
        //enum Conditions
        //{
        //    NotFound = -1,
        //    WithoutStatus = 0,
        //    Success = 1,
        //    NotArrived = 2,
        //    NotSendToCommunication = 3,
        //    SendToCommunication = 4,
        //    CommunicationBlockList = 5,
        //    SendToOperator = 8,
        //    CombackCost = 9,
        //    SendQueue = 10,
        //    Spam = 11,
        //    Expire = 12,
        //    WithoutRoot = 13,
        //    Rejected = 14,
        //    NotArrivedToCommunication = 16,
        //    WithoutDeliveryStatus = 17,

        //}
        //public class JsonModel
        //{
        //    public Boolean result { get; set; }
        //    public String[] list { get; set; }
        //}

        #endregion

        #region Melkavan Profile

        [AllowAnonymous]
        public IActionResult MelkavanProfile()
        {
            //if (this.domainName.Equals("my.teniaco.com"))
            //    return RedirectToAction("Login");


            JsonResultWithRecordsObjectVM jsonResultWithRecordsObjectVM = new JsonResultWithRecordsObjectVM(new object() { });

            var user = consoleBusiness.CmsDb.Users.Where(u => u.UserId.Equals(this.userId.Value)).FirstOrDefault();

            var userProfile = consoleBusiness.CmsDb.UsersProfile.Where(p => p.UserId.Equals(user.UserId)).FirstOrDefault();

            if (ViewData["UserDevicePlatform"] == null)
            {
                ViewData["UserDevicePlatform"] = FrameWork.PlatformInfo.IsMobile(Request.Headers["User-Agent"].ToString()).Equals(true) ? "mobile" : "desktop";
            }

            if (!string.IsNullOrEmpty(userProfile.Name))
            {
                ViewData["Name"] = userProfile.Name;
            }
            else
            {
                ViewData["Name"] = "";
            }

            if (!string.IsNullOrEmpty(userProfile.Email))
            {
                ViewData["Email"] = user.Email;
            }
            else
            {
                ViewData["Email"] = "";
            }


            if (!string.IsNullOrEmpty(userProfile.Family))
            {
                ViewData["Family"] = userProfile.Family;
            }
            else
            {
                ViewData["Family"] = "";
            }


            if (!string.IsNullOrEmpty(userProfile.Mobile))
            {
                ViewData["Mobile"] = userProfile.Mobile;
            }
            else
            {
                ViewData["Mobile"] = "";
            }


            ViewData["CityId"] = userProfile.CityId.ToString();



            if (ViewData["CitiesList"] == null)
            {
                List<CitiesVM> citiesVMList = new List<CitiesVM>();

                try
                {
                    serviceUrl = publicApiUrl + "/api/CitiesManagement/GetAllCitiesList";

                    GetAllCitiesListPVM getAllCitiesListPVM = new GetAllCitiesListPVM()
                    {
                        ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                    this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                    };

                    responseApiCaller = new PublicApiCaller(serviceUrl).GetAllCitiesList(getAllCitiesListPVM);

                    if (responseApiCaller.IsSuccessStatusCode)
                    {
                        jsonResultWithRecordsObjectVM = responseApiCaller.JsonResultWithRecordsObjectVM;

                        if (jsonResultWithRecordsObjectVM != null)
                        {
                            if (jsonResultWithRecordsObjectVM.Result.Equals("OK"))
                            {
                                #region Fill UserCreatorName

                                JArray jArray = jsonResultWithRecordsObjectVM.Records;
                                citiesVMList = jArray.ToObject<List<CitiesVM>>();

                                #endregion
                            }
                        }
                    }
                }
                catch (Exception exc)
                { }

                ViewData["CitiesList"] = citiesVMList;
            }

            //PersonTypeId
            //1 مالک
            //2 مشاور
            if (ViewData["PersonTypeId"] == null)
            {
                var levels = (from l in consoleBusiness.CmsDb.Levels
                              join ul in consoleBusiness.CmsDb.UsersLevels on l.LevelId equals ul.LevelId
                              where ul.UserId.Equals(this.userId.Value)
                              select l).Select(l => l.LevelName).ToList();

                if (levels.Contains("مالک"))
                    ViewData["PersonTypeId"] = 1;
                else
                    if (levels.Contains("مشاور"))
                    ViewData["PersonTypeId"] = 2;
            }



            #region old codes


            //if (ViewData["PersonDetailVM"] == null)
            //{
            //    PersonsVM personsVM = new PersonsVM();

            //    try
            //    {
            //        JsonResultWithRecordObjectVM jsonResultWithRecordObjectVM = new JsonResultWithRecordObjectVM(new object() { });


            //        string PersonServiceUrl = publicApiUrl + "/api/PersonsManagement/GetPersonWithUserId";

            //        GetPersonWithUserIdPVM getPersonWithUserIdPVM =
            //            new GetPersonWithUserIdPVM()
            //            {
            //                //ChildsUsersIds = consoleBusiness.GetDomainChildUserIdsWithStoredProcedure(this.userId.Value, this.domainsSettings.DomainSettingId),
            //                ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
            //                this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
            //                UserId = user.UserId
            //            };
            //        responseApiCaller = new PublicApiCaller(PersonServiceUrl).GetPersonWithUserId(getPersonWithUserIdPVM);

            //        if (responseApiCaller.IsSuccessStatusCode)
            //        {
            //            jsonResultWithRecordObjectVM = responseApiCaller.JsonResultWithRecordObjectVM;

            //            if (jsonResultWithRecordObjectVM != null)
            //            {
            //                if (jsonResultWithRecordObjectVM.Result.Equals("OK"))
            //                {

            //                    JObject jObject = jsonResultWithRecordObjectVM.Record;
            //                    var record = jObject.ToObject<PersonsVM>();

            //                    personsVM = record;
            //                    //personsVM.Email = user.Email;
            //                }
            //            }
            //        }
            //    }
            //    catch (Exception exc)
            //    { }

            //    ViewData["PersonDetailVM"] = personsVM;
            //    ViewData["Email"] = user.Email;

            //}

            //var user = consoleBusiness.GetUserWithUserId(this.userId.Value);


            //ViewData["User"] = userProfile;

            #endregion



            return View("PublicIndex");
        }


        #region Edit Melakavan Profile

        [AllowAnonymous]
        public IActionResult EditMelkavanProfile()
        {
            if (this.domainName.Equals("my.teniaco.com"))
                return RedirectToAction("Login");


            JsonResultWithRecordsObjectVM jsonResultWithRecordsObjectVM = new JsonResultWithRecordsObjectVM(new object() { });

            var user = consoleBusiness.CmsDb.Users.Where(u => u.UserId.Equals(this.userId.Value)).FirstOrDefault();

            var userProfile = consoleBusiness.CmsDb.UsersProfile.Where(p => p.UserId.Equals(user.UserId)).FirstOrDefault();


            if (ViewData["UserDevicePlatform"] == null)
            {
                ViewData["UserDevicePlatform"] = FrameWork.PlatformInfo.IsMobile(Request.Headers["User-Agent"].ToString()).Equals(true) ? "mobile" : "desktop";
            }

            if (ViewData["DomainName"] == null)
                ViewData["DomainName"] = domain;

            ViewData["Email"] = user.Email;
            //ViewData["Email"] = user.Email.ToString();
            ViewData["Name"] = userProfile.Name;
            ViewData["Family"] = userProfile.Family;
            ViewData["Mobile"] = userProfile.Mobile;
            ViewData["UserId"] = user.UserId.ToString();

            ViewData["StateId"] = userProfile.StateId.ToString();
            ViewData["CityId"] = userProfile.CityId.ToString();


            if (ViewData["StatesList"] == null)
            {
                List<StatesVM> statesVMList = new List<StatesVM>();

                try
                {
                    serviceUrl = publicApiUrl + "/api/StatesManagement/GetListOfStates";

                    GetListOfStatesPVM getListOfStatesPVM = new GetListOfStatesPVM()
                    {
                        ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                    this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                    };

                    responseApiCaller = new PublicApiCaller(serviceUrl).GetListOfStates(getListOfStatesPVM);

                    if (responseApiCaller.IsSuccessStatusCode)
                    {
                        jsonResultWithRecordsObjectVM = responseApiCaller.JsonResultWithRecordsObjectVM;

                        if (jsonResultWithRecordsObjectVM != null)
                        {
                            if (jsonResultWithRecordsObjectVM.Result.Equals("OK"))
                            {
                                #region Fill UserCreatorName

                                JArray jArray = jsonResultWithRecordsObjectVM.Records;
                                statesVMList = jArray.ToObject<List<StatesVM>>();


                                #endregion
                            }
                        }
                    }
                }
                catch (Exception exc)
                { }

                ViewData["StatesList"] = statesVMList;
            }

            if (ViewData["CitiesList"] == null)
            {
                List<CitiesVM> citiesVMList = new List<CitiesVM>();

                try
                {
                    serviceUrl = publicApiUrl + "/api/CitiesManagement/GetAllCitiesList";

                    GetAllCitiesListPVM getAllCitiesListPVM = new GetAllCitiesListPVM()
                    {
                        ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                    this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                    };

                    responseApiCaller = new PublicApiCaller(serviceUrl).GetAllCitiesList(getAllCitiesListPVM);

                    if (responseApiCaller.IsSuccessStatusCode)
                    {
                        jsonResultWithRecordsObjectVM = responseApiCaller.JsonResultWithRecordsObjectVM;

                        if (jsonResultWithRecordsObjectVM != null)
                        {
                            if (jsonResultWithRecordsObjectVM.Result.Equals("OK"))
                            {
                                #region Fill UserCreatorName

                                JArray jArray = jsonResultWithRecordsObjectVM.Records;
                                citiesVMList = jArray.ToObject<List<CitiesVM>>();

                                #endregion
                            }
                        }
                    }
                }
                catch (Exception exc)
                { }

                ViewData["CitiesList"] = citiesVMList;
            }

            if (ViewData["PersonTypesList"] == null)
            {
                List<PersonTypesVM> personTypesVMList = new List<PersonTypesVM>();

                try
                {
                    string serviceUrl = publicApiUrl + "/api/PersonTypesManagement/GetAllPersonTypesList";

                    GetAllPersonTypesListPVM getAllPersonTypesListPVM =
                        new GetAllPersonTypesListPVM()
                        {
                            ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                                this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                        };

                    responseApiCaller = new PublicApiCaller(serviceUrl).GetAllPersonTypesList(getAllPersonTypesListPVM);

                    if (responseApiCaller.IsSuccessStatusCode)
                    {
                        jsonResultWithRecordsObjectVM = responseApiCaller.JsonResultWithRecordsObjectVM;

                        if (jsonResultWithRecordsObjectVM != null)
                        {
                            if (jsonResultWithRecordsObjectVM.Result.Equals("OK"))
                            {
                                JArray jArray = jsonResultWithRecordsObjectVM.Records;
                                var records = jArray.ToObject<List<PersonTypesVM>>();

                                personTypesVMList = records;
                            }
                        }
                    }
                }
                catch (Exception exc)
                { }

                ViewData["PersonTypesList"] = personTypesVMList;
            }

            //PersonTypeId
            //1 مالک
            //2 مشاور
            if (ViewData["PersonTypeId"] == null)
            {
                var levels = (from l in consoleBusiness.CmsDb.Levels
                              join ul in consoleBusiness.CmsDb.UsersLevels on l.LevelId equals ul.LevelId
                              where ul.UserId.Equals(this.userId.Value)
                              select l).Select(l => l.LevelName).ToList();

                if (levels.Contains("مالک"))
                    ViewData["PersonTypeId"] = 1;
                else
                    if (levels.Contains("مشاور"))
                    ViewData["PersonTypeId"] = 2;
            }

            dynamic expando = new ExpandoObject();
            expando = user;

            return View("PublicUpdate", expando);

            #region old codes
            //var user = consoleBusiness.GetUserWithUserId(this.userId.Value);
            //JsonResultWithRecordsObjectVM jsonResultWithRecordsObjectVM = new JsonResultWithRecordsObjectVM(new object() { });
            //PersonsVM personsVM = new PersonsVM();

            //try
            //{
            //    JsonResultWithRecordObjectVM jsonResultWithRecordObjectVM = new JsonResultWithRecordObjectVM(new object() { });


            //    string PersonServiceUrl = publicApiUrl + "/api/PersonsManagement/GetPersonWithMobileNumber";

            //    GetPersonWithMobileNumberPVM getPersonWithMobileNumberPVM =
            //        new GetPersonWithMobileNumberPVM()
            //        {
            //            //ChildsUsersIds = consoleBusiness.GetDomainChildUserIdsWithStoredProcedure(this.userId.Value, this.domainsSettings.DomainSettingId),
            //            ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
            //        this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
            //            DomainSettingId = this.domainsSettings.DomainSettingId,
            //            MobileNumber = user.UserName
            //        };
            //    responseApiCaller = new PublicApiCaller(PersonServiceUrl).GetPersonWithMobileNumber(getPersonWithMobileNumberPVM);

            //    if (responseApiCaller.IsSuccessStatusCode)
            //    {
            //        jsonResultWithRecordObjectVM = responseApiCaller.JsonResultWithRecordObjectVM;

            //        if (jsonResultWithRecordObjectVM != null)
            //        {
            //            if (jsonResultWithRecordObjectVM.Result.Equals("OK"))
            //            {

            //                JObject jObject = jsonResultWithRecordObjectVM.Record;
            //                var record = jObject.ToObject<PersonsVM>();
            //                personsVM = record;
            //                //personsVM.Email = user.Email;

            //            }
            //        }
            //    }
            //}
            //catch (Exception exc)
            //{ }
            //ViewData["PersonVM"] = personsVM;



            //if (ViewData["StatesList"] == null)
            //{
            //    List<StatesVM> statesVMList = new List<StatesVM>();

            //    try
            //    {
            //        serviceUrl = publicApiUrl + "/api/StatesManagement/GetListOfStates";

            //        GetListOfStatesPVM getListOfStatesPVM = new GetListOfStatesPVM()
            //        {
            //            ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
            //        this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
            //        };

            //        responseApiCaller = new PublicApiCaller(serviceUrl).GetListOfStates(getListOfStatesPVM);

            //        if (responseApiCaller.IsSuccessStatusCode)
            //        {
            //            jsonResultWithRecordsObjectVM = responseApiCaller.JsonResultWithRecordsObjectVM;

            //            if (jsonResultWithRecordsObjectVM != null)
            //            {
            //                if (jsonResultWithRecordsObjectVM.Result.Equals("OK"))
            //                {
            //                    #region Fill UserCreatorName

            //                    JArray jArray = jsonResultWithRecordsObjectVM.Records;
            //                    statesVMList = jArray.ToObject<List<StatesVM>>();


            //                    if (statesVMList != null)
            //                        if (statesVMList.Count > 0)
            //                        {

            //                        }

            //                    #endregion
            //                }
            //            }
            //        }
            //    }
            //    catch (Exception exc)
            //    { }

            //    ViewData["StatesList"] = statesVMList;
            //}

            //if (ViewData["CitiesList"] == null)
            //{
            //    List<CitiesVM> citiesVMList = new List<CitiesVM>();

            //    try
            //    {
            //        serviceUrl = publicApiUrl + "/api/CitiesManagement/GetAllCitiesList";

            //        GetAllCitiesListPVM getAllCitiesListPVM = new GetAllCitiesListPVM()
            //        {
            //            ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
            //        this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
            //        };

            //        responseApiCaller = new PublicApiCaller(serviceUrl).GetAllCitiesList(getAllCitiesListPVM);

            //        if (responseApiCaller.IsSuccessStatusCode)
            //        {
            //            jsonResultWithRecordsObjectVM = responseApiCaller.JsonResultWithRecordsObjectVM;

            //            if (jsonResultWithRecordsObjectVM != null)
            //            {
            //                if (jsonResultWithRecordsObjectVM.Result.Equals("OK"))
            //                {
            //                    #region Fill UserCreatorName

            //                    JArray jArray = jsonResultWithRecordsObjectVM.Records;
            //                    citiesVMList = jArray.ToObject<List<CitiesVM>>();


            //                    if (citiesVMList != null)
            //                        if (citiesVMList.Count > 0)
            //                        {

            //                        }

            //                    #endregion
            //                }
            //            }
            //        }
            //    }
            //    catch (Exception exc)
            //    { }

            //    ViewData["CitiesList"] = citiesVMList;
            //}

            //if (ViewData["PersonTypesList"] == null)
            //{
            //    List<PersonTypesVM> personTypesVMList = new List<PersonTypesVM>();

            //    try
            //    {
            //        string serviceUrl = publicApiUrl + "/api/PersonTypesManagement/GetAllPersonTypesList";

            //        GetAllPersonTypesListPVM getAllPersonTypesListPVM =
            //            new GetAllPersonTypesListPVM()
            //            {
            //                ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
            //        this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
            //            };

            //        responseApiCaller = new PublicApiCaller(serviceUrl).GetAllPersonTypesList(getAllPersonTypesListPVM);

            //        if (responseApiCaller.IsSuccessStatusCode)
            //        {
            //            jsonResultWithRecordsObjectVM = responseApiCaller.JsonResultWithRecordsObjectVM;

            //            if (jsonResultWithRecordsObjectVM != null)
            //            {
            //                if (jsonResultWithRecordsObjectVM.Result.Equals("OK"))
            //                {
            //                    JArray jArray = jsonResultWithRecordsObjectVM.Records;
            //                    var records = jArray.ToObject<List<PersonTypesVM>>();

            //                    personTypesVMList = records;
            //                }
            //            }
            //        }
            //    }
            //    catch (Exception exc)
            //    { }

            //    ViewData["PersonTypesList"] = personTypesVMList;
            //}


            #endregion

            //  return View("PublicIndex");
        }


        #region old-code Edit profile
        //[HttpPost]
        //[AjaxOnly]
        //public IActionResult EditProfile(PersonsVM personsVM, string email)
        //{
        //    JsonResultWithRecordObjectVM jsonResultWithRecordObjectVM = new JsonResultWithRecordObjectVM(new object() { });

        //    try
        //    {
        //        personsVM.EditEnDate = DateTime.Now;
        //        personsVM.EditTime = PersianDate.TimeNow;
        //        personsVM.UserIdEditor = this.userId.Value;
        //        personsVM.IsActivated = true;
        //        personsVM.IsDeleted = false;

        //        if (ModelState.IsValid)
        //        {
        //            string serviceUrl = melkavanApiUrl + "/api/AdvertisementUserProfileManagement/UpdateUserProfile";

        //            UpdateUserProfilePVM updateUserProfilePVM = new UpdateUserProfilePVM()
        //            {
        //                PersonsVM = personsVM,
        //                Email = email,
        //                //ChildsUsersIds = consoleBusiness.GetDomainChildUserIdsWithStoredProcedure(this.userId.Value,
        //                //this.domainsSettings.DomainSettingId)
        //                ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
        //            this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
        //            };

        //            responseApiCaller = new MelkavanApiCaller(serviceUrl).UpdateUserProfile(updateUserProfilePVM);

        //            if (responseApiCaller.IsSuccessStatusCode)
        //            {
        //                jsonResultWithRecordObjectVM = responseApiCaller.JsonResultWithRecordObjectVM;

        //                if (jsonResultWithRecordObjectVM != null)
        //                {
        //                    if (jsonResultWithRecordObjectVM.Result.Equals("OK"))
        //                    {
        //                        return Json(new
        //                        {
        //                            Result = "OK",
        //                            Message = "ویرایش با موفقیت انجام شد!",
        //                        });
        //                    }
        //                    else
        //                    {
        //                        return Json(new
        //                        {
        //                            Result = "ERROR",
        //                            Message = "خطا"
        //                        });
        //                    }
        //                }
        //            }


        //        }
        //    }
        //    catch (Exception exc)
        //    { }

        //    return Json(new
        //    {
        //        Result = "ERROR",
        //        Message = "ErrorMessage"
        //    });

        //}
        #endregion


        [HttpPost]
        [AjaxOnly]
        public IActionResult EditMelkavanProfile(UpdateUserProfilePVM updateUserProfilePVM
            /*string userId, 
            string email, 
            string stateId, 
            string cityId, 
            string name, 
            string family,
            int personTypeId*/)
        {
            JsonResultWithRecordObjectVM jsonResultWithRecordObjectVM = new JsonResultWithRecordObjectVM(new object() { });

            try
            {

                if (ModelState.IsValid)
                {
                    string serviceUrl = melkavanApiUrl + "/api/AdvertisementUserProfileManagement/UpdateUserProfile";

                    updateUserProfilePVM.ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                            this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds);

                    //UpdateUserProfilePVM updateUserProfilePVM = new UpdateUserProfilePVM()
                    //{
                    //    //UserId = Convert.ToInt64(userId),
                    //    //StateId = Convert.ToInt64(stateId),
                    //    //CityId = Convert.ToInt64(cityId),
                    //    UserId = Convert.ToInt64(userId),
                    //    StateId = Convert.ToInt64(stateId),
                    //    CityId = Convert.ToInt64(cityId),
                    //    Name = name,
                    //    Family = family,
                    //    Email = email,
                    //    PersonTypeId = personTypeId,
                    //    ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                    //        this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                    //};

                    responseApiCaller = new MelkavanApiCaller(serviceUrl).UpdateUserProfile(updateUserProfilePVM);

                    if (responseApiCaller.IsSuccessStatusCode)
                    {
                        jsonResultWithRecordObjectVM = responseApiCaller.JsonResultWithRecordObjectVM;

                        if (jsonResultWithRecordObjectVM != null)
                        {
                            if (jsonResultWithRecordObjectVM.Result.Equals("OK"))
                            {
                                return Json(new
                                {
                                    Result = "OK",
                                    Message = "ویرایش با موفقیت انجام شد!",
                                });
                            }
                            else
                            {
                                return Json(new
                                {
                                    Result = "ERROR",
                                    Message = "خطا"
                                });
                            }
                        }
                    }


                }
            }
            catch (Exception exc)
            { }

            return Json(new
            {
                Result = "ERROR",
                Message = "ErrorMessage"
            });

        }


        #endregion


        #endregion

        #region AdvertisementRegistration

        //صفحه ی  ثبت آگهی
        [AllowAnonymous]
        public IActionResult AdvertisementRegistration()
        {
            //if (this.domainName.Equals("my.teniaco.com"))
            //    return RedirectToAction("Login");

            JsonResultWithRecordsObjectVM jsonResultWithRecordsObjectVM = new JsonResultWithRecordsObjectVM(new object() { });

            string userDevicePlatform = FrameWork.PlatformInfo.IsMobile(Request.Headers["User-Agent"].ToString()).Equals(true) ? "mobile" : "desktop";
            if (ViewData["UserDevicePlatform"] == null)
            {
                ViewData["UserDevicePlatform"] = userDevicePlatform;
            }

            if (ViewData["CurrentYear"] == null)
            {
                ViewData["CurrentYear"] = PersianDate.ThisYear;
            }



            //لیبل ها
            if (ViewData["TagsList"] == null)
            {
                List<TagsVM> tagsVMList = new List<TagsVM>();

                GetAllTagsListPVM getAllTagsListPVM = new GetAllTagsListPVM()
                {
                    ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                    this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                };
                try
                {
                    serviceUrl = melkavanApiUrl + "/api/TagsManagement/GetAllTagsList";

                    responseApiCaller = new MelkavanApiCaller(serviceUrl).GetAllTagsList(getAllTagsListPVM);

                    if (responseApiCaller.IsSuccessStatusCode)
                    {
                        jsonResultWithRecordsObjectVM = responseApiCaller.JsonResultWithRecordsObjectVM;

                        if (jsonResultWithRecordsObjectVM != null)
                        {
                            if (jsonResultWithRecordsObjectVM.Result.Equals("OK"))
                            {

                                JArray jArray = jsonResultWithRecordsObjectVM.Records;
                                tagsVMList = jArray.ToObject<List<TagsVM>>();

                            }
                        }
                    }
                }
                catch (Exception exc)
                { }

                ViewData["TagsList"] = tagsVMList;
            }


            //نوع آگهی
            if (ViewData["AdvertisementTypesList"] == null)
            {
                List<AdvertisementTypesVM> advertisementTypesList = new List<AdvertisementTypesVM>();

                GetAllAdvertisementTypesListPVM getAllAdvertisementTypesListPVM = new GetAllAdvertisementTypesListPVM()
                {
                    ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                    this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                    //ChildsUsersIds = consoleBusiness.GetDomainChildUserIdsWithStoredProcedure(this.userId.Value,
                    //       this.domainsSettings.DomainSettingId),
                };
                try
                {
                    serviceUrl = melkavanApiUrl + "/api/AdvertisementTypesManagement/GetAllAdvertisementTypesList";

                    responseApiCaller = new MelkavanApiCaller(serviceUrl).GetAllAdvertisementTypesList(getAllAdvertisementTypesListPVM);

                    if (responseApiCaller.IsSuccessStatusCode)
                    {
                        jsonResultWithRecordsObjectVM = responseApiCaller.JsonResultWithRecordsObjectVM;

                        if (jsonResultWithRecordsObjectVM != null)
                        {
                            if (jsonResultWithRecordsObjectVM.Result.Equals("OK"))
                            {
                                #region Fill UserCreatorName

                                JArray jArray = jsonResultWithRecordsObjectVM.Records;
                                advertisementTypesList = jArray.ToObject<List<AdvertisementTypesVM>>();



                                #endregion
                            }
                        }
                    }
                }
                catch (Exception exc)
                { }

                ViewData["AdvertisementTypesList"] = advertisementTypesList;
            }


            //نوع ملک
            if (ViewData["PropertyTypesList"] == null)
            {
                List<PropertyTypesVM> propertyTypesVMList = new List<PropertyTypesVM>();

                GetAllPropertyTypesListPVM getAllPropertyTypesListPVM = new GetAllPropertyTypesListPVM()
                {
                    ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                    this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                    //ChildsUsersIds = consoleBusiness.GetDomainChildUserIdsWithStoredProcedure(this.userId.Value,
                    //       this.domainsSettings.DomainSettingId),
                };
                try
                {
                    serviceUrl = teniacoApiUrl + "/api/PropertyTypesManagement/GetAllPropertyTypesList";

                    responseApiCaller = new TeniacoApiCaller(serviceUrl).GetAllPropertyTypesList(getAllPropertyTypesListPVM);

                    if (responseApiCaller.IsSuccessStatusCode)
                    {
                        jsonResultWithRecordsObjectVM = responseApiCaller.JsonResultWithRecordsObjectVM;

                        if (jsonResultWithRecordsObjectVM != null)
                        {
                            if (jsonResultWithRecordsObjectVM.Result.Equals("OK"))
                            {
                                #region Fill UserCreatorName

                                JArray jArray = jsonResultWithRecordsObjectVM.Records;
                                propertyTypesVMList = jArray.ToObject<List<PropertyTypesVM>>();



                                #endregion
                            }
                        }
                    }
                }
                catch (Exception exc)
                { }

                ViewData["PropertyTypesList"] = propertyTypesVMList;
            }


            //نوع کاربری
            if (ViewData["TypeOfUsesList"] == null)
            {
                List<TypeOfUsesVM> typeOfUsesVMList = new List<TypeOfUsesVM>();

                GetAllTypeOfUsesListPVM getAllTypeOfUsesListPVM = new GetAllTypeOfUsesListPVM()
                {
                    ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                    this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                    //ChildsUsersIds = consoleBusiness.GetDomainChildUserIdsWithStoredProcedure(this.userId.Value,
                    //       this.domainsSettings.DomainSettingId),
                };
                try
                {
                    serviceUrl = teniacoApiUrl + "/api/TypeOfUsesManagement/GetAllTypeOfUsesList";

                    responseApiCaller = new TeniacoApiCaller(serviceUrl).GetAllTypeOfUsesList(getAllTypeOfUsesListPVM);

                    if (responseApiCaller.IsSuccessStatusCode)
                    {
                        jsonResultWithRecordsObjectVM = responseApiCaller.JsonResultWithRecordsObjectVM;

                        if (jsonResultWithRecordsObjectVM != null)
                        {
                            if (jsonResultWithRecordsObjectVM.Result.Equals("OK"))
                            {
                                #region Fill UserCreatorName

                                JArray jArray = jsonResultWithRecordsObjectVM.Records;
                                typeOfUsesVMList = jArray.ToObject<List<TypeOfUsesVM>>();

                                #endregion
                            }
                        }
                    }
                }
                catch (Exception exc)
                { }

                ViewData["TypeOfUsesList"] = typeOfUsesVMList;
            }


            //استان
            if (ViewData["StatesList"] == null)
            {
                List<StatesVM> statesVMList = new List<StatesVM>();

                try
                {
                    serviceUrl = publicApiUrl + "/api/StatesManagement/GetListOfStates";

                    GetListOfStatesPVM getListOfStatesPVM = new GetListOfStatesPVM()
                    {
                        //ChildsUsersIds = consoleBusiness.GetDomainChildUserIdsWithStoredProcedure(this.userId.Value,
                        //    this.domainsSettings.DomainSettingId),
                        ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                        this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                    };

                    responseApiCaller = new PublicApiCaller(serviceUrl).GetListOfStates(getListOfStatesPVM);

                    if (responseApiCaller.IsSuccessStatusCode)
                    {
                        jsonResultWithRecordsObjectVM = responseApiCaller.JsonResultWithRecordsObjectVM;

                        if (jsonResultWithRecordsObjectVM != null)
                        {
                            if (jsonResultWithRecordsObjectVM.Result.Equals("OK"))
                            {
                                #region Fill UserCreatorName

                                JArray jArray = jsonResultWithRecordsObjectVM.Records;
                                statesVMList = jArray.ToObject<List<StatesVM>>();


                                #endregion
                            }
                        }
                    }
                }
                catch (Exception exc)
                { }

                ViewData["StatesList"] = statesVMList;
            }


            //بخش
            if (ViewData["CitiesList"] == null)
            {
                List<CitiesVM> citiesVMList = new List<CitiesVM>();

                try
                {
                    serviceUrl = publicApiUrl + "/api/CitiesManagement/GetAllCitiesListWithOutStrPolygon";

                    GetAllCitiesListPVM getAllCitiesListPVM = new GetAllCitiesListPVM()
                    {
                        //ChildsUsersIds = consoleBusiness.GetDomainChildUserIdsWithStoredProcedure(this.userId.Value,
                        //    this.domainsSettings.DomainSettingId),
                        ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                        this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                    };

                    responseApiCaller = new PublicApiCaller(serviceUrl).GetAllCitiesList(getAllCitiesListPVM);

                    if (responseApiCaller.IsSuccessStatusCode)
                    {
                        jsonResultWithRecordsObjectVM = responseApiCaller.JsonResultWithRecordsObjectVM;

                        if (jsonResultWithRecordsObjectVM != null)
                        {
                            if (jsonResultWithRecordsObjectVM.Result.Equals("OK"))
                            {
                                #region Fill UserCreatorName

                                JArray jArray = jsonResultWithRecordsObjectVM.Records;
                                citiesVMList = jArray.ToObject<List<CitiesVM>>();

                                #endregion
                            }
                        }
                    }
                }
                catch (Exception exc)
                { }

                ViewData["CitiesList"] = citiesVMList;
            }


            //شهر یا منطقه
            if (ViewData["ZonesList"] == null)
            {
                List<ZonesVM> zonesVMList = new List<ZonesVM>();

                try
                {
                    string serviceUrl = publicApiUrl + "/api/ZonesManagement/GetAllZonesListWithOutStrPolygon";

                    GetAllZonesListPVM getAllZonesListPVM = new GetAllZonesListPVM()
                    {
                        ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                        this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                        //ChildsUsersIds = consoleBusiness.GetDomainChildUserIdsWithStoredProcedure(this.userId.Value,
                        //   this.domainsSettings.DomainSettingId),
                    };

                    responseApiCaller = new PublicApiCaller(serviceUrl).GetAllZonesList(getAllZonesListPVM);

                    if (responseApiCaller.IsSuccessStatusCode)
                    {
                        jsonResultWithRecordsObjectVM = responseApiCaller.JsonResultWithRecordsObjectVM;

                        if (jsonResultWithRecordsObjectVM != null)
                        {
                            if (jsonResultWithRecordsObjectVM.Result.Equals("OK"))
                            {
                                #region Fill UserCreatorName

                                JArray jArray = jsonResultWithRecordsObjectVM.Records;
                                zonesVMList = jArray.ToObject<List<ZonesVM>>();

                                #endregion
                            }
                        }
                    }
                }
                catch (Exception exc)
                { }

                ViewData["ZonesList"] = zonesVMList;
            }

            //نوع سند
            if (ViewData["DocumentTypesList"] == null)
            {
                List<DocumentTypesVM> documentTypesVMList = new List<DocumentTypesVM>();

                GetAllDocumentTypesListPVM getAllDocumentTypesListPVM = new GetAllDocumentTypesListPVM()
                {
                    ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                    this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                    //ChildsUsersIds = consoleBusiness.GetDomainChildUserIdsWithStoredProcedure(this.userId.Value,
                    //       this.domainsSettings.DomainSettingId),
                };
                try
                {
                    serviceUrl = teniacoApiUrl + "/api/DocumentTypesManagement/GetAllDocumentTypesList";

                    responseApiCaller = new TeniacoApiCaller(serviceUrl).GetAllDocumentTypesList(getAllDocumentTypesListPVM);

                    if (responseApiCaller.IsSuccessStatusCode)
                    {
                        jsonResultWithRecordsObjectVM = responseApiCaller.JsonResultWithRecordsObjectVM;

                        if (jsonResultWithRecordsObjectVM != null)
                        {
                            if (jsonResultWithRecordsObjectVM.Result.Equals("OK"))
                            {
                                #region Fill UserCreatorName

                                JArray jArray = jsonResultWithRecordsObjectVM.Records;
                                documentTypesVMList = jArray.ToObject<List<DocumentTypesVM>>();


                                #endregion
                            }
                        }
                    }
                }
                catch (Exception exc)
                { }

                ViewData["DocumentTypesList"] = documentTypesVMList;
            }


            //نوع ریشه سند
            if (ViewData["DocumentRootTypesList"] == null)
            {
                List<DocumentRootTypesVM> documentRootTypesVMList = new List<DocumentRootTypesVM>();

                GetAllDocumentRootTypesListPVM getAllDocumentRootTypesListPVM = new GetAllDocumentRootTypesListPVM()
                {
                    ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                    this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                    //ChildsUsersIds = consoleBusiness.GetDomainChildUserIdsWithStoredProcedure(this.userId.Value,
                    //       this.domainsSettings.DomainSettingId),
                };
                try
                {
                    serviceUrl = teniacoApiUrl + "/api/DocumentRootTypesManagement/GetAllDocumentRootTypesList";

                    responseApiCaller = new TeniacoApiCaller(serviceUrl).GetAllDocumentRootTypesList(getAllDocumentRootTypesListPVM);

                    if (responseApiCaller.IsSuccessStatusCode)
                    {
                        jsonResultWithRecordsObjectVM = responseApiCaller.JsonResultWithRecordsObjectVM;

                        if (jsonResultWithRecordsObjectVM != null)
                        {
                            if (jsonResultWithRecordsObjectVM.Result.Equals("OK"))
                            {
                                #region Fill UserCreatorName

                                JArray jArray = jsonResultWithRecordsObjectVM.Records;
                                documentRootTypesVMList = jArray.ToObject<List<DocumentRootTypesVM>>();


                                #endregion
                            }
                        }
                    }
                }
                catch (Exception exc)
                { }

                ViewData["DocumentRootTypesList"] = documentRootTypesVMList;
            }


            //نوع مالکیت 
            if (ViewData["DocumentOwnershipTypesList"] == null)
            {
                List<DocumentOwnershipTypesVM> documentOwnershipTypesVMList = new List<DocumentOwnershipTypesVM>();

                GetAllDocumentOwnershipTypesListPVM getAllDocumentOwnershipTypesListPVM = new GetAllDocumentOwnershipTypesListPVM()
                {
                    ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                    this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                    //ChildsUsersIds = consoleBusiness.GetDomainChildUserIdsWithStoredProcedure(this.userId.Value,
                    //       this.domainsSettings.DomainSettingId),
                };
                try
                {
                    serviceUrl = teniacoApiUrl + "/api/DocumentOwnershipTypesManagement/GetAllDocumentOwnershipTypesList";

                    responseApiCaller = new TeniacoApiCaller(serviceUrl).GetAllDocumentOwnershipTypesList(getAllDocumentOwnershipTypesListPVM);

                    if (responseApiCaller.IsSuccessStatusCode)
                    {
                        jsonResultWithRecordsObjectVM = responseApiCaller.JsonResultWithRecordsObjectVM;

                        if (jsonResultWithRecordsObjectVM != null)
                        {
                            if (jsonResultWithRecordsObjectVM.Result.Equals("OK"))
                            {
                                #region Fill UserCreatorName

                                JArray jArray = jsonResultWithRecordsObjectVM.Records;
                                documentOwnershipTypesVMList = jArray.ToObject<List<DocumentOwnershipTypesVM>>();


                                #endregion
                            }
                        }
                    }
                }
                catch (Exception exc)
                { }

                ViewData["DocumentOwnershipTypesList"] = documentOwnershipTypesVMList;
            }


            //عمر بنا
            if (ViewData["BuildingLifesList"] == null)
            {
                List<BuildingLifesVM> buildingLifesList = new List<BuildingLifesVM>();

                GetAllBuildingLifesListPVM getAllBuildingLifesListPVM = new GetAllBuildingLifesListPVM()
                {
                    ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                    this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                };


                try
                {
                    serviceUrl = melkavanApiUrl + "/api/BuildingLifesManagement/GetAllBuildingLifesList";

                    responseApiCaller = new MelkavanApiCaller(serviceUrl).GetAllBuildingLifesList(getAllBuildingLifesListPVM);

                    if (responseApiCaller.IsSuccessStatusCode)
                    {
                        jsonResultWithRecordsObjectVM = responseApiCaller.JsonResultWithRecordsObjectVM;

                        if (jsonResultWithRecordsObjectVM != null)
                        {
                            if (jsonResultWithRecordsObjectVM.Result.Equals("OK"))
                            {
                                #region Fill UserCreatorName

                                JArray jArray = jsonResultWithRecordsObjectVM.Records;
                                buildingLifesList = jArray.ToObject<List<BuildingLifesVM>>();

                                #endregion
                            }
                        }
                    }
                }
                catch (Exception exc)
                { }

                ViewData["BuildingLifesList"] = buildingLifesList;
            }



            AdvertisementVM advertisementVM = new AdvertisementVM();
            advertisementVM.AdvertisementAddressVM = new AdvertisementAddressVM();

            dynamic expando = new ExpandoObject();
            expando = advertisementVM;

            return View("PublicAddTo");

        }



        [HttpPost]
        [AjaxOnly]
        [AllowAnonymous]
        public IActionResult AdvertisementRegistration([FromForm] AdvertisementVM advertisementVM/*, List<IFormFile> filesList*/)
        {
            JsonResultWithRecordObjectVM jsonResultWithRecordObjectVM = new JsonResultWithRecordObjectVM(new object() { });
            JsonResultObjectVM jsonResultObjectVM = new JsonResultObjectVM(new object() { });


            var advertisementFilesVM = new AdvertisementFilesVM();

            try
            {
                var files = advertisementVM.Files;
                string fileName = "";
                string ext = "";

                List<TemporaryAdvertisementFilesVM> myFiles = new List<TemporaryAdvertisementFilesVM>();

                if (files != null)
                {
                    if (files.Count > 0)
                    {
                        var domainSettings = consoleBusiness.GetDomainsSettingsWithUserId(this.userId.Value);
                        domainSettings.DomainName = "melkavan.com";

                        advertisementVM.AdvertisementFilesVM = new List<AdvertisementFilesVM>();

                        foreach (var file in files)
                        {
                            try
                            {
                                ext = Path.GetExtension(file.FileName).ToLower();
                                fileName = Guid.NewGuid().ToString() + ext;

                                advertisementFilesVM = new AdvertisementFilesVM()
                                {
                                    CreateEnDate = DateTime.Now,
                                    CreateTime = PersianDate.TimeNow,
                                    UserIdCreator = this.userId.Value,
                                    IsActivated = true,
                                    IsDeleted = false,
                                    AdvertisementFileExt = ext,
                                    AdvertisementFilePath = fileName,
                                    AdvertisementFileTitle = file.FileName,
                                    AdvertisementFileType = "media",
                                    AdvertisementId = 1,
                                    AdvertisementFileOrder = 1,
                                };


                                advertisementVM.AdvertisementFilesVM.Add(advertisementFilesVM);

                                myFiles.Add(new TemporaryAdvertisementFilesVM()
                                {
                                    AdvertisementFilePath = advertisementFilesVM.AdvertisementFilePath,
                                    MyFile = file
                                });
                            }
                            catch (Exception exc)
                            { }
                        }
                    }
                }

                advertisementVM.CreateEnDate = DateTime.Now;
                advertisementVM.CreateTime = PersianDate.TimeNow;
                advertisementVM.UserIdCreator = this.userId.Value;
                advertisementVM.IsActivated = true;
                advertisementVM.IsDeleted = false;
                if (advertisementVM.AdvertisementDetailsVM.AdvertisementTypeId == 2)//فروش
                {
                    ModelState.Remove("AdvertisementDetailsVM.MaritalStatusId");
                    ModelState.Remove("AdvertisementPricesHistoriesVM.RentPrice");
                    ModelState.Remove("AdvertisementPricesHistoriesVM.DepositPrice");
                    ModelState.Remove("AdvertisementDetailsVM.Convertable");



                }
                else //اجاره
                {
                    ModelState.Remove("DocumentTypeId");
                    ModelState.Remove("DocumentRootTypeId");
                    ModelState.Remove("DocumentOwnershipTypeId");
                    ModelState.Remove("AdvertisementPricesHistoriesVM.OfferPrice");
                }



                ModelState.Remove("BuiltInYear");
                ModelState.Remove("BuiltInYearFa");
                ModelState.Remove("AdvertisementDetailsVM.BuildingLifeId");
                ModelState.Remove("AdvertisementDetailsVM.Foundation");
                ModelState.Remove("AdvertisementAddressVM.Address");
                ModelState.Remove("Files");
                ModelState.Remove("AdvertisementPricesHistoriesVM.CalculatedOfferPrice");


                advertisementVM.Files = null;
                advertisementVM.AdvertisementOwnersVM.OwnerId = this.userId.Value;
                advertisementVM.AdvertisementOwnersVM.Share = 6;
                advertisementVM.AdvertisementOwnersVM.SharePercent = 100;


                if (ModelState.IsValid)
                {
                    string serviceUrl = melkavanApiUrl + "/api/AdvertisementManagement/AddToAdvertisement";

                    AddToAdvertisementPVM addToAdvertisementPVM = new AddToAdvertisementPVM()
                    {
                        /*ChildsUsersIds = consoleBusiness.GetDomainChildUserIdsWithStoredProcedure(this.userId.Value,
                        this.domainsSettings.DomainSettingId),*/
                        ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                         this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                        AdvertisementVM = advertisementVM
                    };

                    responseApiCaller = new MelkavanApiCaller(serviceUrl).AddToAdvertisement(addToAdvertisementPVM);

                    if (responseApiCaller.IsSuccessStatusCode)
                    {
                        jsonResultWithRecordObjectVM = responseApiCaller.JsonResultWithRecordObjectVM;

                        if (jsonResultWithRecordObjectVM != null)
                        {
                            if (jsonResultWithRecordObjectVM.Result.Equals("OK"))
                            {
                                JObject jObject = jsonResultWithRecordObjectVM.Record;
                                var record = jObject.ToObject<AdvertisementVM>();

                                if (record != null)
                                {
                                    advertisementVM = record;

                                    #region create needed folders

                                    if (advertisementVM.AdvertisementId > 0)
                                    {
                                        #region create root folder for this advertisement

                                        try
                                        {
                                            if (myFiles != null)
                                            {
                                                if (myFiles.Count > 0)
                                                {
                                                    advertisementFilesVM.AdvertisementId = advertisementVM.AdvertisementId;

                                                    foreach (var item in advertisementVM.AdvertisementFilesVM)
                                                    {
                                                        item.AdvertisementId = advertisementVM.AdvertisementId;
                                                    }


                                                    //var domainSettings = consoleBusiness.GetDomainsSettingsWithUserId(this.userId.Value);

                                                    string advertisementFolder = hostEnvironment.ContentRootPath + "\\wwwroot\\Files\\AdvertisementsFiles\\" + "melkavan.com" + "\\" + advertisementVM.AdvertisementId + "\\Media";

                                                    if (!Directory.Exists(advertisementFolder))
                                                    {
                                                        Directory.CreateDirectory(advertisementFolder);
                                                    }

                                                    foreach (var myFile in myFiles)
                                                    {
                                                        try
                                                        {
                                                            using (var fileStream = new FileStream(advertisementFolder + "\\" + myFile.AdvertisementFilePath, FileMode.Create))
                                                            {
                                                                myFile.MyFile.CopyToAsync(fileStream);
                                                                System.Threading.Thread.Sleep(100);
                                                            }

                                                            string tmpExt = Path.GetExtension(myFile.AdvertisementFilePath);

                                                            if (tmpExt.Equals(".jpeg") ||
                                                                 tmpExt.Equals(".jpg") ||
                                                                 tmpExt.Equals(".png") ||
                                                                 tmpExt.Equals(".gif") ||
                                                                 tmpExt.Equals(".bmp"))
                                                            {
                                                                ImageModify.ResizeImage(advertisementFolder + "\\",
                                                                    myFile.AdvertisementFilePath,
                                                                    120,
                                                                    120);
                                                            }

                                                        }
                                                        catch (Exception exc)
                                                        { }
                                                    }
                                                }
                                            }
                                        }
                                        catch (Exception exc)
                                        { }

                                        #endregion

                                    }

                                    #endregion

                                    return Json(new
                                    {
                                        Result = "OK",
                                        Record = advertisementVM,
                                        Message = "تعریف انجام شد",
                                    });
                                }
                            }
                            else
                                if (jsonResultWithRecordObjectVM.Result.Equals("ERROR") &&
                                jsonResultWithRecordObjectVM.Message.Equals("DuplicateAdvertisement"))
                            {
                                return Json(new
                                {
                                    Result = "ERROR",
                                    Message = "رکورد تکراری است"
                                });
                            }
                        }
                    }
                }

            }
            catch (Exception exc)
            { }

            return Json(new
            {
                Result = "ERROR",
                Message = "خطا"
            });


        }


        //امکانات یک آگهی
        [AllowAnonymous]
        [AjaxOnly]
        [HttpPost]
        public IActionResult GetAllAdvertisementFeatureValues(GetAllAdvertisementFeatureValuesPVM getAllAdvertisementFeatureValuesPVM)
        {

            if (ViewData["AdvertisementFeaturesValuesVM"] == null)
            {
                AdvertisementFeaturesValuesVM advertisementFeaturesValuesVM = new AdvertisementFeaturesValuesVM();

                JsonResultWithRecordObjectVM jsonResultWithRecordObjectVM = new JsonResultWithRecordObjectVM(new object() { });
                try
                {
                    string serviceUrl = melkavanApiUrl + "/api/AdvertisementFeatureValuesManagement/GetAllAdvertisementFeatureValues";

                    GetAllAdvertisementFeatureValuesPVM getAdvertisementFeaturesValues = new GetAllAdvertisementFeatureValuesPVM()
                    {
                        //ChildsUsersIds = consoleBusiness.GetDomainChildUserIdsWithStoredProcedure(this.domainsSettings.UserIdCreator.Value,
                        //this.domainsSettings.DomainSettingId),
                        ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                                 this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                        AdvertisementTypeId = getAllAdvertisementFeatureValuesPVM.AdvertisementTypeId,
                        PropertyTypeId = getAllAdvertisementFeatureValuesPVM.PropertyTypeId

                    };

                    responseApiCaller = new MelkavanApiCaller(serviceUrl).GetAllAdvertisementFeatureValues(getAllAdvertisementFeatureValuesPVM);

                    if (responseApiCaller.IsSuccessStatusCode)
                    {
                        jsonResultWithRecordObjectVM = responseApiCaller.JsonResultWithRecordObjectVM;

                        if (jsonResultWithRecordObjectVM != null)
                        {
                            if (jsonResultWithRecordObjectVM.Result.Equals("OK"))
                            {
                                #region Fill UserCreatorName

                                JObject jobject = jsonResultWithRecordObjectVM.Record;
                                advertisementFeaturesValuesVM = jobject.ToObject<AdvertisementFeaturesValuesVM>();

                                #endregion
                            }
                        }
                        //ViewData["AdvertisementFeaturesValuesVM"] = advertisementFeaturesValuesVM;
                        return Json(new
                        {
                            jsonResultWithRecordObjectVM.Result,
                            jsonResultWithRecordObjectVM.Record,
                        });
                    }
                }
                catch (Exception exc)
                { }

                //ViewData["AdvertisementFeaturesValuesVM"] = advertisementFeaturesValuesVMList;
            }

            return Json(new
            {
                Result = "ERROR",
                Message = "خطا"
            });
        }


        #endregion

        #region  Avertisement

        //صفحه ی  ویرایش آگهی
        [AllowAnonymous]
        public IActionResult UpdateAdvertisement(int Id, [FromQuery] string type)
        {

            JsonResultWithRecordsObjectVM jsonResultWithRecordsObjectVM = new JsonResultWithRecordsObjectVM(new object() { });

            string userDevicePlatform = FrameWork.PlatformInfo.IsMobile(Request.Headers["User-Agent"].ToString()).Equals(true) ? "mobile" : "desktop";
            if (ViewData["UserDevicePlatform"] == null)
            {
                ViewData["UserDevicePlatform"] = userDevicePlatform;
            }

            if (ViewData["CurrentYear"] == null)
            {
                ViewData["CurrentYear"] = PersianDate.ThisYear;
            }



            //لیبل ها
            if (ViewData["TagsList"] == null)
            {
                List<TagsVM> tagsVMList = new List<TagsVM>();

                GetAllTagsListPVM getAllTagsListPVM = new GetAllTagsListPVM()
                {
                    ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                    this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                };
                try
                {
                    serviceUrl = melkavanApiUrl + "/api/TagsManagement/GetAllTagsList";

                    responseApiCaller = new MelkavanApiCaller(serviceUrl).GetAllTagsList(getAllTagsListPVM);

                    if (responseApiCaller.IsSuccessStatusCode)
                    {
                        jsonResultWithRecordsObjectVM = responseApiCaller.JsonResultWithRecordsObjectVM;

                        if (jsonResultWithRecordsObjectVM != null)
                        {
                            if (jsonResultWithRecordsObjectVM.Result.Equals("OK"))
                            {

                                JArray jArray = jsonResultWithRecordsObjectVM.Records;
                                tagsVMList = jArray.ToObject<List<TagsVM>>();

                            }
                        }
                    }
                }
                catch (Exception exc)
                { }

                ViewData["TagsList"] = tagsVMList;
            }

            //نوع آگهی
            if (ViewData["AdvertisementTypesList"] == null)
            {
                List<AdvertisementTypesVM> advertisementTypesList = new List<AdvertisementTypesVM>();

                GetAllAdvertisementTypesListPVM getAllAdvertisementTypesListPVM = new GetAllAdvertisementTypesListPVM()
                {
                    ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                    this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                    //ChildsUsersIds = consoleBusiness.GetDomainChildUserIdsWithStoredProcedure(this.userId.Value,
                    //       this.domainsSettings.DomainSettingId),
                };
                try
                {
                    serviceUrl = melkavanApiUrl + "/api/AdvertisementTypesManagement/GetAllAdvertisementTypesList";

                    responseApiCaller = new MelkavanApiCaller(serviceUrl).GetAllAdvertisementTypesList(getAllAdvertisementTypesListPVM);

                    if (responseApiCaller.IsSuccessStatusCode)
                    {
                        jsonResultWithRecordsObjectVM = responseApiCaller.JsonResultWithRecordsObjectVM;

                        if (jsonResultWithRecordsObjectVM != null)
                        {
                            if (jsonResultWithRecordsObjectVM.Result.Equals("OK"))
                            {
                                #region Fill UserCreatorName

                                JArray jArray = jsonResultWithRecordsObjectVM.Records;
                                advertisementTypesList = jArray.ToObject<List<AdvertisementTypesVM>>();



                                #endregion
                            }
                        }
                    }
                }
                catch (Exception exc)
                { }

                ViewData["AdvertisementTypesList"] = advertisementTypesList;
            }


            //نوع ملک
            if (ViewData["PropertyTypesList"] == null)
            {
                List<PropertyTypesVM> propertyTypesVMList = new List<PropertyTypesVM>();

                GetAllPropertyTypesListPVM getAllPropertyTypesListPVM = new GetAllPropertyTypesListPVM()
                {
                    ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                    this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                    //ChildsUsersIds = consoleBusiness.GetDomainChildUserIdsWithStoredProcedure(this.userId.Value,
                    //       this.domainsSettings.DomainSettingId),
                };
                try
                {
                    serviceUrl = teniacoApiUrl + "/api/PropertyTypesManagement/GetAllPropertyTypesList";

                    responseApiCaller = new TeniacoApiCaller(serviceUrl).GetAllPropertyTypesList(getAllPropertyTypesListPVM);

                    if (responseApiCaller.IsSuccessStatusCode)
                    {
                        jsonResultWithRecordsObjectVM = responseApiCaller.JsonResultWithRecordsObjectVM;

                        if (jsonResultWithRecordsObjectVM != null)
                        {
                            if (jsonResultWithRecordsObjectVM.Result.Equals("OK"))
                            {
                                #region Fill UserCreatorName

                                JArray jArray = jsonResultWithRecordsObjectVM.Records;
                                propertyTypesVMList = jArray.ToObject<List<PropertyTypesVM>>();



                                #endregion
                            }
                        }
                    }
                }
                catch (Exception exc)
                { }

                ViewData["PropertyTypesList"] = propertyTypesVMList;
            }


            //نوع کاربری
            if (ViewData["TypeOfUsesList"] == null)
            {
                List<TypeOfUsesVM> typeOfUsesVMList = new List<TypeOfUsesVM>();

                GetAllTypeOfUsesListPVM getAllTypeOfUsesListPVM = new GetAllTypeOfUsesListPVM()
                {
                    ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                    this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                    //ChildsUsersIds = consoleBusiness.GetDomainChildUserIdsWithStoredProcedure(this.userId.Value,
                    //       this.domainsSettings.DomainSettingId),
                };
                try
                {
                    serviceUrl = teniacoApiUrl + "/api/TypeOfUsesManagement/GetAllTypeOfUsesList";

                    responseApiCaller = new TeniacoApiCaller(serviceUrl).GetAllTypeOfUsesList(getAllTypeOfUsesListPVM);

                    if (responseApiCaller.IsSuccessStatusCode)
                    {
                        jsonResultWithRecordsObjectVM = responseApiCaller.JsonResultWithRecordsObjectVM;

                        if (jsonResultWithRecordsObjectVM != null)
                        {
                            if (jsonResultWithRecordsObjectVM.Result.Equals("OK"))
                            {
                                #region Fill UserCreatorName

                                JArray jArray = jsonResultWithRecordsObjectVM.Records;
                                typeOfUsesVMList = jArray.ToObject<List<TypeOfUsesVM>>();

                                #endregion
                            }
                        }
                    }
                }
                catch (Exception exc)
                { }

                ViewData["TypeOfUsesList"] = typeOfUsesVMList;
            }


            //استان
            if (ViewData["StatesList"] == null)
            {
                List<StatesVM> statesVMList = new List<StatesVM>();

                try
                {
                    serviceUrl = publicApiUrl + "/api/StatesManagement/GetListOfStates";

                    GetListOfStatesPVM getListOfStatesPVM = new GetListOfStatesPVM()
                    {
                        //ChildsUsersIds = consoleBusiness.GetDomainChildUserIdsWithStoredProcedure(this.userId.Value,
                        //    this.domainsSettings.DomainSettingId),
                        ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                        this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                    };

                    responseApiCaller = new PublicApiCaller(serviceUrl).GetListOfStates(getListOfStatesPVM);

                    if (responseApiCaller.IsSuccessStatusCode)
                    {
                        jsonResultWithRecordsObjectVM = responseApiCaller.JsonResultWithRecordsObjectVM;

                        if (jsonResultWithRecordsObjectVM != null)
                        {
                            if (jsonResultWithRecordsObjectVM.Result.Equals("OK"))
                            {
                                #region Fill UserCreatorName

                                JArray jArray = jsonResultWithRecordsObjectVM.Records;
                                statesVMList = jArray.ToObject<List<StatesVM>>();


                                #endregion
                            }
                        }
                    }
                }
                catch (Exception exc)
                { }

                ViewData["StatesList"] = statesVMList;
            }


            //بخش
            if (ViewData["CitiesList"] == null)
            {
                List<CitiesVM> citiesVMList = new List<CitiesVM>();

                try
                {
                    serviceUrl = publicApiUrl + "/api/CitiesManagement/GetAllCitiesListWithOutStrPolygon";

                    GetAllCitiesListPVM getAllCitiesListPVM = new GetAllCitiesListPVM()
                    {
                        //ChildsUsersIds = consoleBusiness.GetDomainChildUserIdsWithStoredProcedure(this.userId.Value,
                        //    this.domainsSettings.DomainSettingId),
                        ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                        this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                    };

                    responseApiCaller = new PublicApiCaller(serviceUrl).GetAllCitiesList(getAllCitiesListPVM);

                    if (responseApiCaller.IsSuccessStatusCode)
                    {
                        jsonResultWithRecordsObjectVM = responseApiCaller.JsonResultWithRecordsObjectVM;

                        if (jsonResultWithRecordsObjectVM != null)
                        {
                            if (jsonResultWithRecordsObjectVM.Result.Equals("OK"))
                            {
                                #region Fill UserCreatorName

                                JArray jArray = jsonResultWithRecordsObjectVM.Records;
                                citiesVMList = jArray.ToObject<List<CitiesVM>>();

                                #endregion
                            }
                        }
                    }
                }
                catch (Exception exc)
                { }

                ViewData["CitiesList"] = citiesVMList;
            }


            //شهر یا منطقه
            if (ViewData["ZonesList"] == null)
            {
                List<ZonesVM> zonesVMList = new List<ZonesVM>();

                try
                {
                    string serviceUrl = publicApiUrl + "/api/ZonesManagement/GetAllZonesListWithOutStrPolygon";

                    GetAllZonesListPVM getAllZonesListPVM = new GetAllZonesListPVM()
                    {
                        ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                        this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                        //ChildsUsersIds = consoleBusiness.GetDomainChildUserIdsWithStoredProcedure(this.userId.Value,
                        //   this.domainsSettings.DomainSettingId),
                    };

                    responseApiCaller = new PublicApiCaller(serviceUrl).GetAllZonesList(getAllZonesListPVM);

                    if (responseApiCaller.IsSuccessStatusCode)
                    {
                        jsonResultWithRecordsObjectVM = responseApiCaller.JsonResultWithRecordsObjectVM;

                        if (jsonResultWithRecordsObjectVM != null)
                        {
                            if (jsonResultWithRecordsObjectVM.Result.Equals("OK"))
                            {
                                #region Fill UserCreatorName

                                JArray jArray = jsonResultWithRecordsObjectVM.Records;
                                zonesVMList = jArray.ToObject<List<ZonesVM>>();

                                #endregion
                            }
                        }
                    }
                }
                catch (Exception exc)
                { }

                ViewData["ZonesList"] = zonesVMList;
            }

            //نوع سند
            if (ViewData["DocumentTypesList"] == null)
            {
                List<DocumentTypesVM> documentTypesVMList = new List<DocumentTypesVM>();

                GetAllDocumentTypesListPVM getAllDocumentTypesListPVM = new GetAllDocumentTypesListPVM()
                {
                    ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                    this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                    //ChildsUsersIds = consoleBusiness.GetDomainChildUserIdsWithStoredProcedure(this.userId.Value,
                    //       this.domainsSettings.DomainSettingId),
                };
                try
                {
                    serviceUrl = teniacoApiUrl + "/api/DocumentTypesManagement/GetAllDocumentTypesList";

                    responseApiCaller = new TeniacoApiCaller(serviceUrl).GetAllDocumentTypesList(getAllDocumentTypesListPVM);

                    if (responseApiCaller.IsSuccessStatusCode)
                    {
                        jsonResultWithRecordsObjectVM = responseApiCaller.JsonResultWithRecordsObjectVM;

                        if (jsonResultWithRecordsObjectVM != null)
                        {
                            if (jsonResultWithRecordsObjectVM.Result.Equals("OK"))
                            {
                                #region Fill UserCreatorName

                                JArray jArray = jsonResultWithRecordsObjectVM.Records;
                                documentTypesVMList = jArray.ToObject<List<DocumentTypesVM>>();


                                #endregion
                            }
                        }
                    }
                }
                catch (Exception exc)
                { }

                ViewData["DocumentTypesList"] = documentTypesVMList;
            }


            //نوع ریشه سند
            if (ViewData["DocumentRootTypesList"] == null)
            {
                List<DocumentRootTypesVM> documentRootTypesVMList = new List<DocumentRootTypesVM>();

                GetAllDocumentRootTypesListPVM getAllDocumentRootTypesListPVM = new GetAllDocumentRootTypesListPVM()
                {
                    ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                    this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                    //ChildsUsersIds = consoleBusiness.GetDomainChildUserIdsWithStoredProcedure(this.userId.Value,
                    //       this.domainsSettings.DomainSettingId),
                };
                try
                {
                    serviceUrl = teniacoApiUrl + "/api/DocumentRootTypesManagement/GetAllDocumentRootTypesList";

                    responseApiCaller = new TeniacoApiCaller(serviceUrl).GetAllDocumentRootTypesList(getAllDocumentRootTypesListPVM);

                    if (responseApiCaller.IsSuccessStatusCode)
                    {
                        jsonResultWithRecordsObjectVM = responseApiCaller.JsonResultWithRecordsObjectVM;

                        if (jsonResultWithRecordsObjectVM != null)
                        {
                            if (jsonResultWithRecordsObjectVM.Result.Equals("OK"))
                            {
                                #region Fill UserCreatorName

                                JArray jArray = jsonResultWithRecordsObjectVM.Records;
                                documentRootTypesVMList = jArray.ToObject<List<DocumentRootTypesVM>>();


                                #endregion
                            }
                        }
                    }
                }
                catch (Exception exc)
                { }

                ViewData["DocumentRootTypesList"] = documentRootTypesVMList;
            }


            //نوع مالکیت 
            if (ViewData["DocumentOwnershipTypesList"] == null)
            {
                List<DocumentOwnershipTypesVM> documentOwnershipTypesVMList = new List<DocumentOwnershipTypesVM>();

                GetAllDocumentOwnershipTypesListPVM getAllDocumentOwnershipTypesListPVM = new GetAllDocumentOwnershipTypesListPVM()
                {
                    ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                    this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                    //ChildsUsersIds = consoleBusiness.GetDomainChildUserIdsWithStoredProcedure(this.userId.Value,
                    //       this.domainsSettings.DomainSettingId),
                };
                try
                {
                    serviceUrl = teniacoApiUrl + "/api/DocumentOwnershipTypesManagement/GetAllDocumentOwnershipTypesList";

                    responseApiCaller = new TeniacoApiCaller(serviceUrl).GetAllDocumentOwnershipTypesList(getAllDocumentOwnershipTypesListPVM);

                    if (responseApiCaller.IsSuccessStatusCode)
                    {
                        jsonResultWithRecordsObjectVM = responseApiCaller.JsonResultWithRecordsObjectVM;

                        if (jsonResultWithRecordsObjectVM != null)
                        {
                            if (jsonResultWithRecordsObjectVM.Result.Equals("OK"))
                            {
                                #region Fill UserCreatorName

                                JArray jArray = jsonResultWithRecordsObjectVM.Records;
                                documentOwnershipTypesVMList = jArray.ToObject<List<DocumentOwnershipTypesVM>>();


                                #endregion
                            }
                        }
                    }
                }
                catch (Exception exc)
                { }

                ViewData["DocumentOwnershipTypesList"] = documentOwnershipTypesVMList;
            }


            //عمر بنا
            if (ViewData["BuildingLifesList"] == null)
            {
                List<BuildingLifesVM> buildingLifesList = new List<BuildingLifesVM>();

                GetAllBuildingLifesListPVM getAllBuildingLifesListPVM = new GetAllBuildingLifesListPVM()
                {
                    ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                    this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                };


                try
                {
                    serviceUrl = melkavanApiUrl + "/api/BuildingLifesManagement/GetAllBuildingLifesList";

                    responseApiCaller = new MelkavanApiCaller(serviceUrl).GetAllBuildingLifesList(getAllBuildingLifesListPVM);

                    if (responseApiCaller.IsSuccessStatusCode)
                    {
                        jsonResultWithRecordsObjectVM = responseApiCaller.JsonResultWithRecordsObjectVM;

                        if (jsonResultWithRecordsObjectVM != null)
                        {
                            if (jsonResultWithRecordsObjectVM.Result.Equals("OK"))
                            {
                                #region Fill UserCreatorName

                                JArray jArray = jsonResultWithRecordsObjectVM.Records;
                                buildingLifesList = jArray.ToObject<List<BuildingLifesVM>>();

                                #endregion
                            }
                        }
                    }
                }
                catch (Exception exc)
                { }

                ViewData["BuildingLifesList"] = buildingLifesList;
            }



            AdvertisementVM advertisementVM = new AdvertisementVM();


            JsonResultWithRecordObjectVM jsonResultWithRecordObjectVM = new JsonResultWithRecordObjectVM(new object() { });

            try
            {
                string serviceUrl = melkavanApiUrl + "/api/AdvertisementManagement/getAdvertisementWithAdvertisementId";

                GetAdvertisementWithAdvertisementIdPVM getAdvertisementWithAdvertisementIdPVM = new GetAdvertisementWithAdvertisementIdPVM()
                {
                    AdvertisementId = Id,
                    //ChildsUsersIds = consoleBusiness.GetDomainChildUserIdsWithStoredProcedure(this.domainsSettings.UserIdCreator.Value,
                    //    this.domainsSettings.DomainSettingId),
                    ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                            this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                    Type = type
                };

                responseApiCaller = new MelkavanApiCaller(serviceUrl).GetAdvertisementWithAdvertisementId(getAdvertisementWithAdvertisementIdPVM);

                if (responseApiCaller.IsSuccessStatusCode)
                {
                    jsonResultWithRecordObjectVM = responseApiCaller.JsonResultWithRecordObjectVM;

                    if (jsonResultWithRecordObjectVM != null)
                    {
                        if (jsonResultWithRecordObjectVM.Result.Equals("OK"))
                        {
                            JObject jObject = jsonResultWithRecordObjectVM.Record;
                            var record = jObject.ToObject<AdvertisementVM>();


                            if (record != null)
                            {
                                //advertisementVM.AdvertisementFilesVM = advertisementVM.AdvertisementFilesVM.OrderByDescending(a => a.AdvertisementFileId).ToList();
                                advertisementVM = record;
                            }
                        }
                    }
                }
            }
            catch (Exception exc)
            { }


            dynamic expando = new ExpandoObject();
            expando = advertisementVM;

            return View("PublicUpdate", expando);

        }



        [HttpPost]
        [AjaxOnly]
        [AllowAnonymous]
        public IActionResult UpdateAdvertisement([FromForm] AdvertisementVM advertisementVM/*, List<IFormFile> filesList*/)
        {
            JsonResultWithRecordObjectVM jsonResultWithRecordObjectVM = new JsonResultWithRecordObjectVM(new object() { });
            JsonResultObjectVM jsonResultObjectVM = new JsonResultObjectVM(new object() { });


            var advertisementFilesVM = new AdvertisementFilesVM();

            try
            {
                var files = advertisementVM.Files;
                string fileName = "";
                string ext = "";

                List<TemporaryAdvertisementFilesVM> myFiles = new List<TemporaryAdvertisementFilesVM>();

                //for adding photos
                if (files != null)
                {
                    if (files.Count > 0)
                    {
                        var domainSettings = consoleBusiness.GetDomainsSettingsWithUserId(this.userId.Value);
                        domainSettings.DomainName = "melkavan.com";

                        advertisementVM.AdvertisementFilesVM = new List<AdvertisementFilesVM>();

                        foreach (var file in files)
                        {
                            try
                            {
                                ext = Path.GetExtension(file.FileName).ToLower();
                                fileName = Guid.NewGuid().ToString() + ext;

                                advertisementFilesVM = new AdvertisementFilesVM()
                                {
                                    CreateEnDate = DateTime.Now,
                                    CreateTime = PersianDate.TimeNow,
                                    UserIdCreator = this.userId.Value,
                                    IsActivated = true,
                                    IsDeleted = false,
                                    AdvertisementFileExt = ext,
                                    AdvertisementFilePath = fileName,
                                    AdvertisementFileTitle = file.FileName,
                                    AdvertisementFileType = "media",
                                    AdvertisementId = 1,
                                    AdvertisementFileOrder = 1,
                                };


                                advertisementVM.AdvertisementFilesVM.Add(advertisementFilesVM);

                                myFiles.Add(new TemporaryAdvertisementFilesVM()
                                {
                                    AdvertisementFilePath = advertisementFilesVM.AdvertisementFilePath,
                                    MyFile = file
                                });
                            }
                            catch (Exception exc)
                            { }
                        }
                    }
                }

                //for removing photos
                if (advertisementVM.DeletedPhotosPaths != null)
                {
                    foreach (string path in advertisementVM.DeletedPhotosPaths)
                    {
                        string fullPath = hostEnvironment.ContentRootPath + "\\wwwroot" + path.Replace("/", "\\");

                        if (System.IO.File.Exists(fullPath))
                        {
                            System.IO.File.Delete(fullPath);
                        }

                        string thumbPath = fullPath.Replace("\\Media\\", "\\Media\\thumb_");

                        if (System.IO.File.Exists(thumbPath))
                        {
                            System.IO.File.Delete(thumbPath);
                        }
                    }
                }

                //advertisementVM.CreateEnDate = DateTime.Now;
                //advertisementVM.CreateTime = PersianDate.TimeNow;
                advertisementVM.UserIdEditor = this.userId.Value;
                //advertisementVM.IsActivated = true;
                //advertisementVM.IsDeleted = false;
                if (advertisementVM.AdvertisementDetailsVM.AdvertisementTypeId == 2)//فروش
                {
                    ModelState.Remove("AdvertisementDetailsVM.MaritalStatusId");
                    ModelState.Remove("AdvertisementPricesHistoriesVM.RentPrice");
                    ModelState.Remove("AdvertisementPricesHistoriesVM.DepositPrice");
                    ModelState.Remove("AdvertisementDetailsVM.Convertable");



                }
                else //اجاره
                {
                    ModelState.Remove("DocumentTypeId");
                    ModelState.Remove("DocumentRootTypeId");
                    ModelState.Remove("DocumentOwnershipTypeId");
                    ModelState.Remove("AdvertisementPricesHistoriesVM.OfferPrice");
                }



                ModelState.Remove("BuiltInYear");
                ModelState.Remove("BuiltInYearFa");
                ModelState.Remove("AdvertisementDetailsVM.BuildingLifeId");
                ModelState.Remove("AdvertisementDetailsVM.Foundation");
                ModelState.Remove("AdvertisementAddressVM.Address");
                ModelState.Remove("Files");
                ModelState.Remove("AdvertisementPricesHistoriesVM.CalculatedOfferPrice");
                ModelState.Remove("DeletedPhotosIDs");
                ModelState.Remove("DeletedPhotosPaths");
                ModelState.Remove("IsMainChanged");


                advertisementVM.Files = null;
                //advertisementVM.AdvertisementOwnersVM.OwnerId = this.userId.Value;
                //advertisementVM.AdvertisementOwnersVM.Share = 6;
                //advertisementVM.AdvertisementOwnersVM.SharePercent = 100;


                if (ModelState.IsValid)
                {
                    string serviceUrl = melkavanApiUrl + "/api/AdvertisementManagement/UpdateAdvertisement";

                    AddToAdvertisementPVM addToAdvertisementPVM = new AddToAdvertisementPVM()
                    {
                        /*ChildsUsersIds = consoleBusiness.GetDomainChildUserIdsWithStoredProcedure(this.userId.Value,
                        this.domainsSettings.DomainSettingId),*/
                        ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                         this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                        AdvertisementVM = advertisementVM
                    };

                    responseApiCaller = new MelkavanApiCaller(serviceUrl).AddToAdvertisement(addToAdvertisementPVM);

                    if (responseApiCaller.IsSuccessStatusCode)
                    {
                        jsonResultWithRecordObjectVM = responseApiCaller.JsonResultWithRecordObjectVM;

                        if (jsonResultWithRecordObjectVM != null)
                        {
                            if (jsonResultWithRecordObjectVM.Result.Equals("OK"))
                            {
                                JObject jObject = jsonResultWithRecordObjectVM.Record;
                                var record = jObject.ToObject<AdvertisementVM>();

                                if (record != null)
                                {
                                    advertisementVM = record;

                                    #region create needed folders

                                    if (advertisementVM.AdvertisementId > 0)
                                    {
                                        #region create root folder for this advertisement

                                        try
                                        {
                                            if (myFiles != null)
                                            {
                                                if (myFiles.Count > 0)
                                                {
                                                    advertisementFilesVM.AdvertisementId = advertisementVM.AdvertisementId;

                                                    foreach (var item in advertisementVM.AdvertisementFilesVM)
                                                    {
                                                        item.AdvertisementId = advertisementVM.AdvertisementId;
                                                    }


                                                    //var domainSettings = consoleBusiness.GetDomainsSettingsWithUserId(this.userId.Value);

                                                    string advertisementFolder = "";
                                                    if (advertisementVM.RecordType == "Advertisement")
                                                    {
                                                        advertisementFolder = hostEnvironment.ContentRootPath + "\\wwwroot\\Files\\AdvertisementsFiles\\" + "melkavan.com" + "\\" + advertisementVM.AdvertisementId + "\\Media";
                                                    }
                                                    else
                                                    {
                                                        advertisementFolder = hostEnvironment.ContentRootPath + "\\wwwroot\\Files\\PropertiesFiles\\" + "my.teniaco.com" + "\\" + advertisementVM.AdvertisementId + "\\Media";
                                                    }


                                                    if (!Directory.Exists(advertisementFolder))
                                                    {
                                                        Directory.CreateDirectory(advertisementFolder);
                                                    }

                                                    foreach (var myFile in myFiles)
                                                    {
                                                        try
                                                        {
                                                            using (var fileStream = new FileStream(advertisementFolder + "\\" + myFile.AdvertisementFilePath, FileMode.Create))
                                                            {
                                                                myFile.MyFile.CopyToAsync(fileStream);
                                                                System.Threading.Thread.Sleep(100);
                                                            }

                                                            string tmpExt = Path.GetExtension(myFile.AdvertisementFilePath);

                                                            if (tmpExt.Equals(".jpeg") ||
                                                                 tmpExt.Equals(".jpg") ||
                                                                 tmpExt.Equals(".png") ||
                                                                 tmpExt.Equals(".gif") ||
                                                                 tmpExt.Equals(".bmp"))
                                                            {
                                                                ImageModify.ResizeImage(advertisementFolder + "\\",
                                                                    myFile.AdvertisementFilePath,
                                                                    120,
                                                                    120);
                                                            }

                                                        }
                                                        catch (Exception exc)
                                                        { }
                                                    }
                                                }
                                            }
                                        }
                                        catch (Exception exc)
                                        { }

                                        #endregion

                                    }

                                    #endregion

                                    return Json(new
                                    {
                                        Result = "OK",
                                        Record = advertisementVM,
                                        Message = "ویرایش انجام شد",
                                    });
                                }
                            }
                            else
                                if (jsonResultWithRecordObjectVM.Result.Equals("ERROR") &&
                                jsonResultWithRecordObjectVM.Message.Equals("DuplicateAdvertisement"))
                            {
                                return Json(new
                                {
                                    Result = "ERROR",
                                    Message = ""
                                });
                            }
                        }
                    }
                }

            }
            catch (Exception exc)
            { }

            return Json(new
            {
                Result = "ERROR",
                Message = "خطا"
            });


        }

        #endregion

        #region MyAdvertisement

        //آگهی های من
        [AllowAnonymous]
        public IActionResult MyAdvertisements()
        {
            if (ViewData["UserDevicePlatform"] == null)
            {
                ViewData["UserDevicePlatform"] = FrameWork.PlatformInfo.IsMobile(Request.Headers["User-Agent"].ToString()).Equals(true) ? "mobile" : "desktop";
            }
            var user = consoleBusiness.GetUserWithUserId(this.userId.Value);

            //  ViewData["DomainName"] = consoleBusiness.GetDomainsSettingsWithUserId(this.userId.Value).DomainName;
            ViewData["DomainName"] = "melkavan.com";

            return View("PublicIndex");
        }


        [HttpPost]
        [AjaxOnly]
        [AllowAnonymous]
        public IActionResult MyAdvertisements(int startIndex, int pageSize)
        {
            List<AdvertisementVM> advertisementVMs = new List<AdvertisementVM>();
            var user = consoleBusiness.GetUserWithUserId(this.userId.Value);

            try
            {
                JsonResultWithRecordsObjectVM jsonResultWithRecordsObjectVM = new JsonResultWithRecordsObjectVM(new object() { });


                string ServiceUrl = melkavanApiUrl + "/api/AdvertisementManagement/GetAdvertisementsWithOwnerId";

                GetAdvertisementsWithOwnerIdPVM getAdvertisementsWithOwnerIdPVM =
                    new GetAdvertisementsWithOwnerIdPVM()
                    {
                        //ChildsUsersIds = consoleBusiness.GetDomainChildUserIdsWithStoredProcedure(this.userId.Value, this.domainsSettings.DomainSettingId),
                        ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                          this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                        OwnerId = user.UserId,
                        jtPageSize = pageSize,
                        jtStartIndex = startIndex,
                    };
                responseApiCaller = new MelkavanApiCaller(ServiceUrl).GetAdvertisementsWithOwnerId(getAdvertisementsWithOwnerIdPVM);

                if (responseApiCaller.IsSuccessStatusCode)
                {
                    jsonResultWithRecordsObjectVM = responseApiCaller.JsonResultWithRecordsObjectVM;

                    if (jsonResultWithRecordsObjectVM != null)
                    {
                        if (jsonResultWithRecordsObjectVM.Result.Equals("OK"))
                        {

                            JArray jArray = jsonResultWithRecordsObjectVM.Records;
                            var records = jArray.ToObject<List<AdvertisementAdvanceSearchVM>>();
                            return Json(new
                            {
                                Result = "OK",
                                Records = records,
                                Message = "Success"
                            });
                        }
                    }
                }
            }
            catch (Exception exc)
            { }
            return Json(new
            {
                Result = "ERROR",
                Message = "خطا"
            });


        }

        #endregion

        #region AvertisementsViewers

        //بازدید آگهی ها
        [AllowAnonymous]
        public IActionResult WatchedAdvertisements()
        {
            if (ViewData["UserDevicePlatform"] == null)
            {
                ViewData["UserDevicePlatform"] = FrameWork.PlatformInfo.IsMobile(Request.Headers["User-Agent"].ToString()).Equals(true) ? "mobile" : "desktop";
            }
            var user = consoleBusiness.GetUserWithUserId(this.userId.Value);

            // ViewData["DomainName"] = consoleBusiness.GetDomainsSettingsWithUserId(this.userId.Value).DomainName;
            ViewData["DomainName"] = "melkavan.com";

            return View("PublicIndex");
        }


        [HttpPost]
        [AjaxOnly]
        [AllowAnonymous]
        public IActionResult WatchedAdvertisements(int startIndex, int pageSize)
        {
            List<AdvertisementVM> advertisementVMs = new List<AdvertisementVM>();
            var user = consoleBusiness.GetUserWithUserId(this.userId.Value);

            try
            {
                JsonResultWithRecordsObjectVM jsonResultWithRecordsObjectVM = new JsonResultWithRecordsObjectVM(new object() { });


                string ServiceUrl = melkavanApiUrl + "/api/AdvertisementManagement/GetWatchedAdvertisementsWithOwnerId";

                GetAdvertisementsWithOwnerIdPVM getAdvertisementsWithOwnerIdPVM =
                    new GetAdvertisementsWithOwnerIdPVM()
                    {
                        //ChildsUsersIds = consoleBusiness.GetDomainChildUserIdsWithStoredProcedure(this.userId.Value, this.domainsSettings.DomainSettingId),
                        ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                          this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                        OwnerId = user.UserId,
                        jtPageSize = pageSize,
                        jtStartIndex = startIndex,
                    };
                responseApiCaller = new MelkavanApiCaller(ServiceUrl).GetWatchedAdvertisementsWithOwnerId(getAdvertisementsWithOwnerIdPVM);

                if (responseApiCaller.IsSuccessStatusCode)
                {
                    jsonResultWithRecordsObjectVM = responseApiCaller.JsonResultWithRecordsObjectVM;

                    if (jsonResultWithRecordsObjectVM != null)
                    {
                        if (jsonResultWithRecordsObjectVM.Result.Equals("OK"))
                        {

                            JArray jArray = jsonResultWithRecordsObjectVM.Records;
                            var records = jArray.ToObject<List<AdvertisementAdvanceSearchVM>>();
                            return Json(new
                            {
                                Result = "OK",
                                Records = records,
                                Message = "Success"
                            });
                        }
                    }
                }
            }
            catch (Exception exc)
            { }
            return Json(new
            {
                Result = "ERROR",
                Message = "خطا"
            });


        }

        #endregion

        #region AdvertisementDetails

        //جزئیات آگهی
        [AllowAnonymous]
        public IActionResult AdvertisementDetails(long Id, string Type)
        {
            //if (this.domainName.Equals("my.teniaco.com"))
            //    return RedirectToAction("Login");


            if (ViewData["UserDevicePlatform"] == null)
            {
                ViewData["UserDevicePlatform"] = FrameWork.PlatformInfo.IsMobile(Request.Headers["User-Agent"].ToString()).Equals(true) ? "mobile" : "desktop";
            }

            if (ViewData["AdvertisementVM"] == null)
            {
                AdvertisementVM advertisementVM = new AdvertisementVM();

                try
                {
                    JsonResultWithRecordObjectVM jsonResultWithRecordObjectVM = new JsonResultWithRecordObjectVM(new object() { });


                    string ServiceUrl = melkavanApiUrl + "/api/AdvertisementManagement/GetAdvertisementWithAdvertisementId";

                    GetAdvertisementWithAdvertisementIdPVM getAdvertisementWithAdvertisementIdPVM =
                        new GetAdvertisementWithAdvertisementIdPVM()
                        {
                            ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                             this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                            AdvertisementId = Id,
                            Type = Type,
                            UserId = this.userId.Value
                        };
                    responseApiCaller = new MelkavanApiCaller(ServiceUrl).GetAdvertisementWithAdvertisementId(getAdvertisementWithAdvertisementIdPVM);

                    if (responseApiCaller.IsSuccessStatusCode)
                    {
                        jsonResultWithRecordObjectVM = responseApiCaller.JsonResultWithRecordObjectVM;

                        if (jsonResultWithRecordObjectVM != null)
                        {
                            if (jsonResultWithRecordObjectVM.Result.Equals("OK"))
                            {

                                JObject jObject = jsonResultWithRecordObjectVM.Record;
                                var record = jObject.ToObject<AdvertisementVM>();

                                advertisementVM = record;
                                //personsVM.Email = user.Email;
                            }
                        }
                    }
                }
                catch (Exception exc)
                { }

                ViewData["AdvertisementVM"] = advertisementVM;

                try
                {
                    JsonResultWithRecordObjectVM jsonResultWithRecordObjectVM = new JsonResultWithRecordObjectVM(new object() { });


                    string ServiceUrl = melkavanApiUrl + "/api/AdvertisementViewersManagement/AddToAdvertisementViewers";

                    AddToAdvertisementViewersPVM addToAdvertisementViewersPVM =
                        new AddToAdvertisementViewersPVM()
                        {
                            AdvertisementViewersVM = new AdvertisementViewersVM
                            {
                                AdvertisementId = Id,
                                CreateEnDate = DateTime.Now,
                                CreateTime = PersianDate.TimeNow,
                                UserIdCreator = (this.userId.HasValue && userId.Value > 0) ? this.userId.Value : null,
                                Type = Type
                            },
                            ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                    this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds)
                        };
                    responseApiCaller = new MelkavanApiCaller(ServiceUrl).AddToAdvertisementViewers(addToAdvertisementViewersPVM);

                    if (responseApiCaller.IsSuccessStatusCode)
                    {
                        jsonResultWithRecordObjectVM = responseApiCaller.JsonResultWithRecordObjectVM;

                        if (jsonResultWithRecordObjectVM != null)
                        {
                            if (jsonResultWithRecordObjectVM.Result.Equals("OK"))
                            {

                            }
                        }
                    }
                }
                catch (Exception exc)
                { }

            }


            // ViewData["DomainName"] = this.domainsSettings.DomainName;
            ViewData["DomainName"] = "melkavan.com";

            return View("PublicIndex");
        }


        //صفحه ی تماس
        [AjaxOnly]
        [HttpPost]
        [AllowAnonymous]
        public IActionResult CallAdRegisterer(long id, string type)
        {
            try
            {
                if (this.userId.Value != 0)
                {


                    JsonResultWithRecordObjectVM jsonResultWithRecordObjectVM = new JsonResultWithRecordObjectVM(new object() { });


                    string ServiceUrl = melkavanApiUrl + "/api/AdvertisementCallersManagement/AddToAdvertisementCallers";

                    AddToAdvertisementCallersPVM addToAdvertisementCallersPVM =
                        new AddToAdvertisementCallersPVM()
                        {
                            AdvertisementCallersVM = new AdvertisementCallersVM
                            {
                                AdvertisementId = id,
                                AdvertisementCallerType = type,
                                UserIdCreator = this.userId.Value,
                                CreateEnDate = DateTime.Now,
                                CreateTime = PersianDate.TimeNow,
                            },
                            ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                                this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                        };
                    responseApiCaller = new MelkavanApiCaller(ServiceUrl).AddToAdvertisementCallers(addToAdvertisementCallersPVM);

                    if (responseApiCaller.IsSuccessStatusCode)
                    {
                        jsonResultWithRecordObjectVM = responseApiCaller.JsonResultWithRecordObjectVM;

                        if (jsonResultWithRecordObjectVM != null)
                        {
                            if (jsonResultWithRecordObjectVM.Result.Equals("OK"))
                            {
                                JObject jObject = jsonResultWithRecordObjectVM.Record;
                                var record = jObject.ToObject<AdvertisementCallersVM>();
                                return Json(new
                                {
                                    Result = "OK",
                                    Records = record,
                                    Message = "Success"
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception exc)
            { }

            return Json(new
            {
                Result = "ERROR",
                Message = "خطا"
            });

        }


        #endregion       

        #region AboutMelkavanPage
        //درباره ی ملکاوان
        [AllowAnonymous]
        public IActionResult AboutMelkavan()
        {
            if (ViewData["UserDevicePlatform"] == null)
            {
                ViewData["UserDevicePlatform"] = FrameWork.PlatformInfo.IsMobile(Request.Headers["User-Agent"].ToString()).Equals(true) ? "mobile" : "desktop";
            }
            return View("PublicIndex");
        }
        #endregion

        #region RuleOfMelkavanPage
        //قوانبن ملکاوان

        [AllowAnonymous]
        public IActionResult Rules()
        {
            if (ViewData["UserDevicePlatform"] == null)
            {
                ViewData["UserDevicePlatform"] = FrameWork.PlatformInfo.IsMobile(Request.Headers["User-Agent"].ToString()).Equals(true) ? "mobile" : "desktop";
            }
            return View("PublicIndex");
        }
        #endregion

        #region AdvertisementFavorite
        [AjaxOnly]
        [HttpPost]
        [AllowAnonymous]
        public IActionResult AdvertisementFavorite(long id, string type)
        {
            try
            {
                JsonResultWithRecordObjectVM jsonResultWithRecordObjectVM = new JsonResultWithRecordObjectVM(new object() { });


                string ServiceUrl = melkavanApiUrl + "/api/AdvertisementFavoritesManagement/AddToAdvertisementFavorites";

                var user = consoleBusiness.GetUserWithUserId(this.userId.Value);

                AddToAdvertisementFavoritesPVM addToAdvertisementFavoritesPVM =
                    new AddToAdvertisementFavoritesPVM()
                    {
                        AdvertisementFavoritesVM = new AdvertisementFavoritesVM
                        {
                            AdvertisementId = id,
                            CreateEnDate = DateTime.Now,
                            CreateTime = PersianDate.TimeNow,
                            UserIdCreator = user.UserId,
                            Type = type
                        },
                        ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                             this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                    };
                responseApiCaller = new MelkavanApiCaller(ServiceUrl).AddToAdvertisementFavorites(addToAdvertisementFavoritesPVM);

                if (responseApiCaller.IsSuccessStatusCode)
                {
                    jsonResultWithRecordObjectVM = responseApiCaller.JsonResultWithRecordObjectVM;

                    if (jsonResultWithRecordObjectVM != null)
                    {
                        if (jsonResultWithRecordObjectVM.Result.Equals("OK"))
                        {
                            return Json(new
                            {
                                Result = "OK",
                                Record = jsonResultWithRecordObjectVM.Record,
                                Message = "Success"
                            });
                        }
                    }
                }
            }
            catch (Exception exc)
            { }
            return Json(new
            {
                Result = "ERROR",
                Message = "خطا"
            });

        }
        #endregion

        #region ViewChart
        //نمودار بازدید


        [AllowAnonymous]
        public IActionResult ViewChart([FromRoute] int Id, [FromQuery] string Type)
        {
            JsonResultWithRecordsObjectVM jsonResultWithRecordsObjectVM = new JsonResultWithRecordsObjectVM(new object() { });

            if (ViewData["UserDevicePlatform"] == null)
            {
                ViewData["UserDevicePlatform"] = FrameWork.PlatformInfo.IsMobile(Request.Headers["User-Agent"].ToString()).Equals(true) ? "mobile" : "desktop";
            }

            string ServiceUrl = melkavanApiUrl + "/api/AdvertisementViewersManagement/GetAdvertisementViewersWithAdvertisementId";

            var user = consoleBusiness.GetUserWithUserId(this.userId.Value);
            GetAdvertisementViewersWithAdvertisementIdPVM getAdvertisementViewersWithAdvertisementIdPVM =
                new GetAdvertisementViewersWithAdvertisementIdPVM()
                {
                    ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                      this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                    UserId = user.UserId,
                    AdvertisementId = Id,
                    RecordType = Type
                };
            responseApiCaller = new MelkavanApiCaller(ServiceUrl).GetAdvertisementViewersWithAdvertisementId(getAdvertisementViewersWithAdvertisementIdPVM);

            if (responseApiCaller.IsSuccessStatusCode)
            {
                jsonResultWithRecordsObjectVM = responseApiCaller.JsonResultWithRecordsObjectVM;

                if (jsonResultWithRecordsObjectVM != null)
                {
                    if (jsonResultWithRecordsObjectVM.Result.Equals("OK"))
                    {

                        JArray jArray = jsonResultWithRecordsObjectVM.Records;
                        var records = jArray.ToObject<List<AdvertisementViewersVM>>();
                        ViewData["Viewers"] = records;
                    }
                }
            }


            return View("PublicIndex");
        }
        #endregion

        #region DeleteAdvertisement

        [HttpPost]
        [AjaxOnly]
        [AllowAnonymous]
        public IActionResult DeleteAdvertisement(int advertisementId, string type)
        {
            var user = consoleBusiness.GetUserWithUserId(this.userId.Value);
            try
            {

                string ServiceUrl = melkavanApiUrl + "/api/AdvertisementManagement/CompleteDeleteAdvertisement";

                CompleteDeleteAdvertisementPVM getAdvertisementById =
                    new CompleteDeleteAdvertisementPVM()
                    {
                        //ChildsUsersIds = consoleBusiness.GetDomainChildUserIdsWithStoredProcedure(this.userId.Value, this.domainsSettings.DomainSettingId),
                        ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                          this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                        AdvertisementId = advertisementId,
                        UserId = user.UserId,
                        Type = type
                    };
                responseApiCaller = new MelkavanApiCaller(ServiceUrl).CompleteDeleteAdvertisement(getAdvertisementById);

                if (responseApiCaller.IsSuccessStatusCode)
                {

                    if (type.Equals("Advertisement"))
                    {

                        var folderPath = Path.Combine(
                            hostEnvironment.ContentRootPath,
                            "wwwroot",
                            "Files",
                            "AdvertisementsFiles",
                            "melkavan.com",
                            advertisementId.ToString(),
                            "Media");


                        if (Directory.Exists(folderPath))
                        {
                            Directory.Delete(folderPath, true);
                        }

                    }


                    return Json(new
                    {
                        Result = "OK",
                        Message = "Success"
                    });
                }
            }
            catch (Exception exc)
            { }
            return Json(new
            {
                Result = "ERROR",
                Message = "خطا"
            });


        }

        #endregion

        #region PriceMap

        [AllowAnonymous]
        public IActionResult PriceMap()
        {
            if (ViewData["UserDevicePlatform"] == null)
            {
                ViewData["UserDevicePlatform"] = FrameWork.PlatformInfo.IsMobile(Request.Headers["User-Agent"].ToString()).Equals(true) ? "mobile" : "desktop";
            }

            JsonResultWithRecordsObjectVM jsonResultWithRecordsObjectVM = new JsonResultWithRecordsObjectVM(new object() { });

            try
            {
                string serviceUrl = teniacoApiUrl + "/api/MapLayerCategoriesManagement/GetListOfPropertiesPricesForMap";

                GetListOfPropertiesPricesForMapPVM getListOfPropertiesPricesForMapPVM = new GetListOfPropertiesPricesForMapPVM()
                {
                    ChildsUsersIds = consoleBusiness.GetChildUserIdsMatchWithLevelIds(this.areaName, this.controllerName, this.actionName, this.userId.Value, this.parentUserId.Value,
                    this.domainsSettings.UserIdCreator.Value, this.roleIds, this.levelIds),
                    Platform = 3
                };

                responseApiCaller = new TeniacoApiCaller(serviceUrl).GetListOfPropertiesPricesForMap(getListOfPropertiesPricesForMapPVM);

                if (responseApiCaller.IsSuccessStatusCode)
                {
                    jsonResultWithRecordsObjectVM = responseApiCaller.JsonResultWithRecordsObjectVM;

                    if (jsonResultWithRecordsObjectVM != null)
                    {
                        if (jsonResultWithRecordsObjectVM.Result.Equals("OK"))
                        {

                            JArray jArray = jsonResultWithRecordsObjectVM.Records;
                            var records = jArray.ToObject<List<PropertiesPricesForMapVM>>();


                            ViewData["Records"] = records;
                            ViewData["Result"] = jsonResultWithRecordsObjectVM.Result;

                        }
                    }
                }
            }
            catch (Exception exc)
            { }

            ViewData["DomainName"] = "melkavan.com";

            return View("PublicIndex");
        }

        #endregion
    }
}