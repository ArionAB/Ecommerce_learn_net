﻿using Ecommerce.DataLayer.Utils;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace Ecommerce.DataLayer.DTOs.Baby
{
    public class UpdateBabyItemDTO
    {
        public Guid BabyId { get; set; }

        public string Price { get; set; }

        public string BabySize { get; set; }

        public string Description { get; set; }

        public string Title { get; set; }

        public CategoryType CategoryType { get; set; }

        public List<IFormFile> NewAdditionalPictures { get; set; }

        public List<Guid> DeletedAdditionalPictures { get; set; }


    }
}
