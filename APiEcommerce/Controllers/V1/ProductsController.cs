using APiEcommerce.Models;
using APiEcommerce.Models.Dtos;
using APiEcommerce.Repository.IRepository;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APiEcommerce.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersionNeutral]
    [ApiController]
    // [Authorize]
    [Authorize(Roles ="Admin")]//esto define que role tiene que tener para acceder
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productsRepository;
        private readonly IMapper _mapper;
        private readonly ICategoryRepository _categoryRepository;
        public ProductsController(IProductRepository productsRepository, IMapper mapper, ICategoryRepository categoryRepository)
        {
            _mapper = mapper;
            _productsRepository = productsRepository;
            _categoryRepository = categoryRepository;
        }


        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProducts()
        {
            var Products = _productsRepository.GetProducts();
            var ProductsDto = _mapper.Map<List<ProductDto>>(Products);

            return Ok(ProductsDto);
        }
        [AllowAnonymous]
        [HttpGet("{ProductId:int}", Name = "GetProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProduct(int ProductId)
        {
            var Product = _productsRepository.GetProduct(ProductId);
            if (Product == null) return NotFound($"El product con el Id {ProductId} no existe");

            var ProductDto = _mapper.Map<ProductDto>(Product);
            return Ok(ProductDto);
        }

        [AllowAnonymous]
        [HttpGet("Paged", Name = "GetProductsInPage")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProductsInPage([FromQuery]int pageNumber=1,[FromQuery]int pageSize=5)
        {
            if (pageNumber < 1 || pageSize < 1) return BadRequest("los parametros no pueden ser menores a 1");
            var totalProduct = _productsRepository.GetTotalProducts();
            var totalPages = (int)Math.Ceiling((double) totalProduct / pageSize);            

            if (pageNumber > totalPages) return NotFound($"No se encontro la pagina");
            var Product = _productsRepository.GetProductsInPages(pageNumber, pageSize);

            var ProductDto = _mapper.Map<List<ProductDto>>(Product);
            var paginationResponse = new PaginationResponse<ProductDto>
            {
                PageNumbre = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
                Items = ProductDto
            };
            return Ok(paginationResponse);
        }
        [HttpGet("searchByCategory/{CategoryId:int}", Name = "GetProductsForCategory")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProductsForCategory(int CategoryId)
        {
            var Product = _productsRepository.GetProductsForCategory(CategoryId);
            if (Product.Count == 0) return NotFound($"no se encontraron prroductos con la categoria {CategoryId}");

            var ProductsDto = _mapper.Map<List<ProductDto>>(Product);

            return Ok(ProductsDto);
        }
        [HttpGet("searchProductByNameDescription/{NamwOrdescrition}", Name = "GetProductsForNameOrDescrition")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProductsForNameOrDescrition(string NamwOrdescrition)
        {
            var Product = _productsRepository.SearchProducts(NamwOrdescrition);
            if (Product.Count == 0) return NotFound($"no se encontraron prroductos con el nombre o descripcion {NamwOrdescrition}");

            var ProductsDto = _mapper.Map<List<ProductDto>>(Product);

            return Ok(ProductsDto);
        }
        [HttpPatch("buyProduct/{name}/{quantity:int}", Name = "buyProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProductsForNameOrDescrition(string name, int quantity)
        {
            if (string.IsNullOrWhiteSpace(name) || quantity <= 0) return BadRequest("el nombre del producto o la cantidad no son validos");
            if (!_productsRepository.ProductExists(name))
            {
                return NotFound($"el Producto con el nombre {name} no existe");
            }
            if (!_productsRepository.BuyProduct(name, quantity))
            {
                ModelState.AddModelError("CustomError", $"No se pudocomprar el producto {name} o la cantidad solicitada es mayor a la disponible");
                return BadRequest(ModelState);
            }
            var unit = quantity == 1 ? "unidad" : "unidades";
            return Ok($"se compro {quantity} {unit} del producto {name}");


        }
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public IActionResult CreateProduct([FromForm] CreateProductDto createProductDto)
        {
            if (createProductDto == null) return BadRequest(ModelState);

            if (_productsRepository.ProductExists(createProductDto.Name))
            {
                ModelState.AddModelError("CustomError", $"el producta con el nombre {createProductDto.Name} ya existe");
                return BadRequest(ModelState);
            }
            if (!_categoryRepository.CategoryExists(createProductDto.CategoryId))
            {
                ModelState.AddModelError("CustomError", $"la categoria {createProductDto.CategoryId} no existe");
                return BadRequest(ModelState);
            }

            var Product = _mapper.Map<Product>(createProductDto);
            /// agregando imagen 
            if (createProductDto.Image != null)
            {
                UploadProductImages(createProductDto, Product);
            }
            else
            {
                Product.ImgUrl = "https://placehold.co/300x300";
            }
            if (!_productsRepository.CreateProduct(Product))
            {
                ModelState.AddModelError("CustomError", $"Algo salio mal al guardar el regidtro {Product.Name}");
                return StatusCode(500, ModelState);
            }
            var createProduct = _productsRepository.GetProduct(Product.ProductId);
            var productDto = _mapper.Map<ProductDto>(createProduct);
            return CreatedAtRoute("GetProduct", new { ProductId = Product.ProductId }, productDto);
        }

        private void UploadProductImages(dynamic ProductDto, Product Product)
        {
            string fileNAme = Product.ProductId + Guid.NewGuid().ToString() + Path.GetExtension(ProductDto.Image.FileName);
            var imegesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ProductsImages");
            if (!Directory.Exists(imegesFolder)) Directory.CreateDirectory(imegesFolder);
            var filePath = Path.Combine(imegesFolder, fileNAme);
            FileInfo file = new FileInfo(filePath);
            if (file.Exists) file.Delete();
            using var fileStream = new FileStream(filePath, FileMode.Create);
            ProductDto.Image.CopyTo(fileStream);
            var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
            Product.ImgUrl = $"{baseUrl}/productsImages/{fileNAme}";
            Product.ImgUrlLocal = filePath;
        }

        [HttpPut("{id:int}", Name = "UpdateProduct")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult UpdateProduct(int id, [FromForm] CreateProductDto updateProductDto)
        {
            if (updateProductDto == null) return BadRequest(ModelState);


            if (!_productsRepository.ProductExists(id))
            {
                ModelState.AddModelError("CustomError", "El product no existe");
                return BadRequest(ModelState);
            }
            if (!_categoryRepository.CategoryExists(updateProductDto.CategoryId))
            {
                ModelState.AddModelError("CustomError", $"la categoria {updateProductDto.CategoryId} no existe");
                return BadRequest(ModelState);
            }

            if (_productsRepository.ProductExists(updateProductDto.Name))
            {
                ModelState.AddModelError("CustomError", "el product ya existe ");
                return BadRequest(ModelState);
            }
            var Product = _mapper.Map<Product>(updateProductDto);
            Product.ProductId = id;

        //update Imagen 
            if (updateProductDto.Image != null)
            {
               UploadProductImages(updateProductDto, Product);
            }
            else
            {
                Product.ImgUrl = "https://placehold.co/300x300";
            }
            if (!_productsRepository.UpdateProduct(Product))
            {
                ModelState.AddModelError("CustomError", $"Algo salio mal al actualizar el regidtro {Product.Name}");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }

        [HttpDelete("{id:int}", Name = "DeleteProduct")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult DeleteProduct(int id)
        {

            if (id == 0) return BadRequest(ModelState);

            if (!_productsRepository.ProductExists(id))
            {
                ModelState.AddModelError("CustomError", "el Product ya existe ");
                return BadRequest(ModelState);
            }
            var product = _productsRepository.GetProduct(id);
            if (product == null) return NotFound($"el Product con el Id {id} no existe");

            if (!_productsRepository.DeleteProduct(product))
            {
                ModelState.AddModelError("CustomError", $"Algo salio mal al eliminar el regidtro {product.Name}");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }

    }
}
