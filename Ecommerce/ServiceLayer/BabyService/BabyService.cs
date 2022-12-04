﻿using AutoMapper;
using Ecommerce.DataLayer.DbContexts;
using Ecommerce.DataLayer.DTOs.Baby;
using Ecommerce.DataLayer.Models.Baby;
using Ecommerce.DataLayer.Utils;
using Ecommerce.ServiceLayer.FileService;
using Ecommerce.ServiceLayer.Utils;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.ServiceLayer.BabyService
{
    public class BabyService : IBabyService
    {
        private readonly MainDbContext _context;
        private readonly IMapper _mapper;
        private readonly IFileService _fileService;

        public BabyService(MainDbContext context, IMapper mapper, IFileService fileService)
        {
            _context = context;
            _mapper = mapper;
            _fileService = fileService;
        }

        public async Task<ServiceResponse<Object>> AddBabyItem(AddBabyItemDTO babyItemDTO)
        {

            try
            {
                var baby = _mapper.Map<BabyClass>(babyItemDTO);
                await _context.Baby.AddAsync(baby);

                if (babyItemDTO.Pictures != null && babyItemDTO.Pictures.Count > 0)
                {

                    var paths = await _fileService.UploadPictures(babyItemDTO.Pictures, FilePaths.GetAdditionalFilesPaths(baby.BabyId));

                    if (!paths.Success)
                    {
                        return new ServiceResponse<Object> { Response = (string)null, Success = false, Message = Messages.Message_UploadPictureSuccess };
                    }

                    //foreach (var file in bodysuit.Pictures)
                    foreach (var file in baby.BabyPictures)
                    {
                        file.FilePath = paths.Response[baby.BabyPictures.ToList().IndexOf(file)];
                    }


                }

                foreach (var item in babyItemDTO.BabySize)
                {
                    
                    if (baby.BabySizes.Where(x => x.Size == item.Size).Count() == 0)
                    {
                        baby.BabySizes.Add(new BabySize { Size = item.Size, Quantity = item.Quantity });
                    }
                }
              
            

                //save bodysuit
                _context.SaveChanges();

                return new ServiceResponse<Object> { Response = (string)null, Success = true, Message = Messages.Message_UploadPictureSuccess };

            }
            catch (Exception e)
            {
                return new ServiceResponse<Object> { Response = (string)null, Success = false, Message = Messages.Message_UploadPictureError };
            }

        }

        public async Task<ServiceResponse<List<BabyDTO>>> GetBabyItems()
        {
            try
            {
                var babyItems = _mapper.ProjectTo<BabyDTO>(_context.Baby).ToList();
                //var babySizes = _mapper.ProjectTo<BabySizeDTO>(_context.BabySizes).ToList();


                var counted = new List<BabyDTO>();

                {
                    foreach (var item in babyItems)
                    {
                        var count = _context.Baby.Where(x => x.CategoryType == item.CategoryType).Count();
                        //var totalSize = _context.Baby.Where(x => x.CategoryType == item.CategoryType && x.BabySize == item.BabySize).Count();
                        var totalSize = _context.Baby.Where(x => x.CategoryType == item.CategoryType).Count();
                        //var totalQuantity = _context.BabySizes.Where(x => x.BabyId == item.BabyId && x.BabySizeId != item.BabyId && x.Quantity > 0).Sum(x => x.Quantity);
                        var totalQuantity = _context.BabySizes.Add(new BabySize { Size = "2", Quantity = 5 });


                        counted = _mapper.ProjectTo<BabyDTO>(_context.Baby).ToList();


                        //counted.Add(new BabyDTO
                        //{
                        //    BabyId = item.BabyId,
                        //    CategoryType = item.CategoryType,
                        //    Description = item.Description,
                        //    Title = item.Title,
                        //    Price = item.Price,

                        //    //Quantity = item.Quantity,
                        //    //BabySize = item.BabySize,
                        //    TotalItems = count,
                        //    TotalSize = totalSize,
                        //    BabySizes = totalQuantity

                        //});
                    }

                    //foreach (var item in babySizes)
                    //{
                    //    counted = babySizes.Where(x => x.Size == item.Size).ToList();
                    //}
                }



                return new ServiceResponse<List<BabyDTO>> { Response = counted, Success = true, Message = Messages.Message_GetBabyItemsSuccess };
            }
            catch (Exception e)
            {
                return new ServiceResponse<List<BabyDTO>> { Response = null, Success = false, Message = Messages.Message_GetBabyItemsError };
            }
        }

        public async Task<ServiceResponse<BabyDTO>> GetBabyItem(Guid id)
        {
            try
            {
                if (GenericFunctions.GuidIsNullOrEmpty(id))
                {
                    return new ServiceResponse<BabyDTO> { Response = null, Success = false, Message = Messages.Message_GetBabyItemIdError };
                }

                var babyItem = _mapper.ProjectTo<BabyDTO>(_context.Baby).FirstOrDefault(b => b.BabyId == id);

                return new ServiceResponse<BabyDTO> { Response = babyItem, Success = true, Message = Messages.Message_GetBabyItemSuccess };
            }
            catch (Exception e)
            {
                return new ServiceResponse<BabyDTO> { Response = null, Success = false, Message = Messages.Message_GetBabyItemError };
            }
        }

        public async Task<ServiceResponse<BabyDTO>> UpdateBabyItem(UpdateBabyItemDTO babyitemDTO)
        {
            try
            {
                var babyItem = _context.Baby.FirstOrDefault(x => x.BabyId == babyitemDTO.BabyId);
                //daca foloseam si include el imi adauga si pathurile existente si imi crapa la index

                var simpleBabyItem = _mapper.Map<BabyDTO>(babyItem);

                _mapper.Map(babyitemDTO, babyItem);
                if (babyitemDTO.NewAdditionalPictures != null && babyitemDTO.NewAdditionalPictures.Count > 0)
                {
                    var paths = await _fileService.UploadPictures(babyitemDTO.NewAdditionalPictures, FilePaths.GetAdditionalFilesPaths(babyItem.BabyId));
                    foreach (var file in (babyItem.BabyPictures))
                    {
                        file.FilePath = paths.Response[babyItem.BabyPictures.ToList().IndexOf(file)];
                    }
                }

                if (babyitemDTO.DeletedAdditionalPictures != null && babyitemDTO.DeletedAdditionalPictures.Count > 0)
                {
                    var pictures = _context.BabyPictures.Where(x => babyitemDTO.DeletedAdditionalPictures.Contains(x.PictureId)).ToList();
                    _context.BabyPictures.RemoveRange(pictures);
                    _fileService.DeleteAdditionalPictures(pictures.Select(x => x.FilePath).ToList());

                }

                _context.Baby.Update(babyItem);

                _context.SaveChanges();

                var result = _mapper.Map<BabyDTO>(babyItem);

                return new ServiceResponse<BabyDTO> { Response = result, Success = true, Message = Messages.Message_UpdateBabyItemSuccess };

            }
            catch (Exception e)
            {
                return new ServiceResponse<BabyDTO> { Response = null, Success = false, Message = Messages.Message_UpdateBabyItemError };
            }
        }

        private List<BabyDTO> getBabyItemsFiltered(BabyFiltersDTO filter)
        {
            
            var babyItems = new List<BabyDTO>();

            switch (filter.Category)
            {
                case CategoryType.All:

                    try
                    {


                        //foreach (var size in filter.BabySize)
                        //{
                        //    //babyItems.AddRange(_mapper.ProjectTo<BabyDTO>(_context.BabySizes).Where(x => x.BabySizes.Any(y => y.Size == size.ToString())).ToList());
                        //    var babySizes = _mapper.ProjectTo<BabyDTO>(_context.BabySizes).Where(x => x.BabySizes.Any(y => y.Size == size.ToString())).ToList();
                        //}
                        //var babySizes = _context.BabySizes.Where(x => x.Size == filter.BabySize.ToString());
                        //babyItems.AddRange((IEnumerable<BabyDTO>)babySizes);
                        //if (filter.BabySize != null)
                            //foreach(var size in filter.BabySize)
                            //{
                            //    {
                            //            //babyItems.AddRange(_mapper.ProjectTo<BabyDTO>(_context.BabySizes).Where(x => x.BabySizes.Any(y => y.Size == size.ToString())).ToList());
                            //        }
                            //}
                            babyItems = _mapper.ProjectTo<BabyDTO>(_context.Baby
                            .Where(x => filter.MinPrice <= x.Price)
                            .Where(x => filter.MaxPrice >= x.Price)
                            //.Where(x => _context.BabySizes.Where(x => x.Size == filter.BabySize.ToString()).Contains(x.BabySize)
                            //.Where(x => _context.BabySizes.Where(x => x.Size == filter.BabySize.ToString()).ToList().Contains(x.BabyId)
                            .Take(filter.PageSize)).ToList();

                        //.Where(x => filter.BabySize == x.BabySizes)

                        //.Where(x => filter.BabySize.Contains(x.BabySize))
                        //.Where(x => filter.BabySize[0] != 0 ? filter.BabySize.Count == 0 || filter.BabySize.Contains(x.BabySize) : true)




                    return babyItems;

                    }
                    catch (Exception e)
                    {
                        return babyItems;
                    }
                //var babyItems = new List<BabyFiltersDto>
                //{
                //    var babyItems = _mapper.ProjectTo<BabyDTO>(_context.Baby
                //    .Where(x => filter.MinPrice <= x.Price)
                //    .Where(x => filter.MaxPrice >= x.Price)
                //    .Where(x => filter.BabySize == null || x.BabySizes.Any(y => y.Size == filter.BabySize))
                //    .Take(filter.PageSize)).ToList();
                //}

                case CategoryType.Bodysuit:
                    babyItems = _mapper.ProjectTo<BabyDTO>(_context.Baby.Where(x => x.CategoryType == CategoryType.Bodysuit)
                        .Where(x => filter.MinPrice <= x.Price)
                        .Where(x => filter.MaxPrice >= x.Price)
                        //.Where(x => filter.BabySize[0] != 0 ? filter.BabySize.Count == 0 || filter.BabySize.Contains(x.BabySize) : true)
                        .Take(filter.PageSize)).ToList();

                    return babyItems;

                case CategoryType.Coverall:
                    babyItems = _mapper.ProjectTo<BabyDTO>(_context.Baby.Where(x => x.CategoryType == CategoryType.Coverall)
                        .Where(x => filter.MinPrice <= x.Price)
                        .Where(x => filter.MaxPrice >= x.Price)
                        //.Where(x => filter.BabySize[0] != 0 ? filter.BabySize.Count == 0 || filter.BabySize.Contains(x.BabySize) : true)
                        .Take(filter.PageSize)).ToList();


                    return babyItems;
           
            }

            return babyItems;
           
            
        }

        public async Task<ServiceResponse<PaginatedBabyItemsDTO>> GetPaginatedBabyItems(BabyFiltersDTO filters)
        {
            try
            {
                var babyItems = getBabyItemsFiltered(filters);

                var counted = new List<BabyDTO>();

                {
                    foreach (var item in babyItems)
                    {
                        var totalItems = babyItems.Contains(item) ? babyItems.Count : 0;


                        var totalSize = babyItems.Contains(item) ? _context.Baby.Where(x => x.CategoryType == item.CategoryType && babyItems.Contains(item)).Count() : 0;
                        //var totalSize = babyItems.Contains(item) ? _context.Baby.Where(x => x.CategoryType == item.CategoryType && x.BabySize == item.BabySize && babyItems.Contains(item)).Count() : 0;



                        counted.Add(new BabyDTO
                        {
                            BabyId = item.BabyId,
                            CategoryType = item.CategoryType,
                            Description = item.Description,
                            Title = item.Title,
                            Price = item.Price,
                            BabyPictures = item.BabyPictures,
                            BabySizes = item.BabySizes,
                            //Quantity = item.Quantity,
                            //BabySize = item.BabySize,
                            TotalItems = totalItems,
                            TotalSize = totalSize
                        });
                    }
                }

                var totalRecords = _context.Baby

                     .Where(x => filters.MinPrice <= x.Price)
                     .Where(x => filters.MaxPrice >= x.Price).Count();
                     //.Where(x => filters.BabySize[0] != 0 ? filters.BabySize.Count == 0 || filters.BabySize.Contains(x.BabySize) : true).Count();

                var totalPages = totalRecords / filters.PageSize;
                if (totalRecords == filters.PageSize) totalPages--;

                var response = new PaginatedBabyItemsDTO
                {
                    BabyItems = counted,
                    TotalPages = totalPages,
                    CurrentPageNumber = filters.PageNumber,
                    TotalItems = totalRecords
                    
                };

                return new ServiceResponse<PaginatedBabyItemsDTO> { Response = response, Success = true };
            }
            catch(Exception e)
            {
                return new ServiceResponse<PaginatedBabyItemsDTO> { Response = null, Success = false, Message = Messages.Message_GetPaginatedBabyItemsError };
            }
        }







    }
    
}
