using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Dtos;
using API.Errors;
using API.Helpers;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Core.Specifications;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Neo4jClient;


namespace API.Controllers
{
    public class SinhVien {
        public int MaSo { get; set; }
        public string HoTen { get; set; }
        public string Caption { get; set; }
        public string Sex { get; set; }  
    }

    public class ProductsController : BaseApiController
    {
        private readonly IPhotoService _photoService;
        private readonly IGraphClient _neo4JService;

        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProductService _productService;
        public ProductsController(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService, IProductService productService, IGraphClient neo4JService)
        {
            _photoService = photoService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _productService = productService;
            _neo4JService = neo4JService;
        }


        [Cached(600)]
        [HttpGet]
        public async Task<ActionResult<Pagination<ProductToReturnDto>>> GetProducts(
            [FromQuery] ProductSpecParams productParams)
        {
            var spec = new ProductsWithTypesAndBrandsSpecification(productParams);
            var countSpec = new ProductWithFiltersForCountSpecification(productParams);
            //var totalItems = await _unitOfWork.Repository<Product>().CountAsync(countSpec);
            var totalItems = await _productService.CountAsync(countSpec);
            //var products = await _unitOfWork.Repository<Product>().ListAsync(spec);
            var products = await _productService.ListAsync(spec);

            var data = _mapper
                .Map<IReadOnlyList<Product>, IReadOnlyList<ProductToReturnDto>>(products);
            return Ok(new Pagination<ProductToReturnDto>(productParams.PageIndex,
            productParams.PageSize, totalItems, data));
        }


        [Cached(600)]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductToReturnDto>> GetProduct(string id)
        {
            var spec = new ProductsWithTypesAndBrandsSpecification(id);
            //var product = await _unitOfWork.Repository<Product>().GetEntityWithSpec(spec);
            var product = await _productService.GetEntityWithSpec(spec);

            if (product == null) return NotFound(new ApiResponse(404));
            return _mapper.Map<Product, ProductToReturnDto>(product);
        }


        [Cached(600)]
        [HttpGet("brands")]
        public async Task<ActionResult<IReadOnlyList<ProductBrand>>> GetProductBrands()
        {
            //return Ok(await _unitOfWork.Repository<ProductBrand>().ListAllAsync());
            return Ok(await _productService.GetProductBrandsAsync());
        }


        [Cached(600)]
        [HttpGet("types")]
        public async Task<ActionResult<IReadOnlyList<ProductType>>> GetProductTypes()
        {
            return Ok(await _productService.GetProductTypesAsync());
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Product>> CreateProduct(ProductCreateDto productToCreate)
        {
            var product = _mapper.Map<ProductCreateDto, Product>(productToCreate);
            try
            {
                await _productService.Add(product);
            }
            catch (Exception)
            {
                return BadRequest(new ApiResponse(400, "Problem creating product"));
            }
            //var result = await _unitOfWork.Complete();

            return Ok(product);
        }


        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Product>> UpdateProduct(string id, ProductCreateDto productToUpdate)
        {
            var product = await _productService.GetByIdAsync(id);

            _mapper.Map(productToUpdate, product);
            try
            {
                await _productService.Update(product);
            }
            catch(Exception)
            {
                return BadRequest(new ApiResponse(400, "Problem updating product"));
            }
            //var result = await _unitOfWork.Complete();

            //if (result <= 0) 
            return Ok(product);
        }


        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Product>> DeleteProduct(string id)
        {
            var product = await _productService.GetByIdAsync(id);
            foreach (var photo in product.Photos)
            {
                if (photo.Id > 18)
                {
                    _photoService.DeleteFromDisk(photo);
                }
            }

            try
            {
                await _productService.Delete(product);
            }
            catch (Exception)
            {
                return BadRequest(new ApiResponse(400, "Problem deleting product"));
            }
            return Ok(product);
        }


        [HttpPut("{id}/photo")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProductToReturnDto>> AddProductPhoto(string id, [FromForm] ProductPhotoDto photoDto)
        {
            var spec = new ProductsWithTypesAndBrandsSpecification(id);
            var product = await _productService.GetEntityWithSpec(spec);

            if (photoDto.Photo.Length > 0)
            {
                var photo = await _photoService.SaveToDiskAsync(photoDto.Photo);

                if (photo != null)
                {
                    product.AddPhoto(photo.PictureUrl, photo.FileName);
                    try
                    {
                        await _productService.Update(product);
                    }
                    catch (Exception)
                    {
                        return BadRequest(new ApiResponse(400, "Problem adding photo product"));
                    }
                }
                else
                {
                    return BadRequest(new ApiResponse(400, "problem saving photo to disk"));
                }
            }

            return _mapper.Map<Product, ProductToReturnDto>(product);
        }


        [HttpDelete("{id}/photo/{photoId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteProductPhoto(string id, int photoId)
        {
            var spec = new ProductsWithTypesAndBrandsSpecification(id);
            var product = await _productService.GetEntityWithSpec(spec);

            var photo = product.Photos.SingleOrDefault(x => x.Id == photoId);

            if (photo != null)
            {
                if (photo.IsMain)
                    return BadRequest(new ApiResponse(400,
                        "You cannot delete the main photo"));

                _photoService.DeleteFromDisk(photo);
            }
            else
            {
                return BadRequest(new ApiResponse(400, "Photo does not exist"));
            }

            product.RemovePhoto(photoId);

            try
            {
                await _productService.Update(product);
            }
            catch (Exception)
            {
                return BadRequest(new ApiResponse(400, "Problem deleting photo product"));
            }
            return Ok();
        }



        [HttpPost("{id}/photo/{photoId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProductToReturnDto>> SetMainPhoto(string id, int photoId)
        {
            var spec = new ProductsWithTypesAndBrandsSpecification(id);
            var product = await _productService.GetEntityWithSpec(spec);

            if (product.Photos.All(x => x.Id != photoId)) return NotFound();

            product.SetMainPhoto(photoId);


            try
            {
                await _productService.Update(product);
            }
            catch (Exception)
            {
                return BadRequest(new ApiResponse(400, "Problem setting photo as main"));
            }
            return _mapper.Map<Product, ProductToReturnDto>(product);
        }


        [HttpPost("exec/{command}")]
        public async Task<IActionResult> Exec(string command)
        {
            try
            {
                await _neo4JService.ConnectAsync();
                if (!_neo4JService.IsConnected)
                    return new BadRequestObjectResult("Not connected");


                var result = await _neo4JService.Cypher.Match("(sv:SINHVIEN)").Return(sv => sv.As<SinhVien>()).ResultsAsync;
                

                return Ok(result.ToList());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }
    }
}