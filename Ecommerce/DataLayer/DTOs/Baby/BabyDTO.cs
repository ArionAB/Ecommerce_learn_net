﻿using Ecommerce.DataLayer.Utils;
using Microsoft.AspNetCore.Http;
using System;

namespace Ecommerce.DataLayer.DTOs.Baby
{
    public class BabyDTO
    {
        public Guid BabyId { get; set; }

        public BabySizeType BabySize { get; set; }

        public int Price { get; set; }

        public string Description { get; set; }

        public string Title { get; set; }

        public CategoryType CategoryType { get; set; }




    }
}
