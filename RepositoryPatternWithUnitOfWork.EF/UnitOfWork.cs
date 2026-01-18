using RepositoryPatternWithUnitOfWork.Core;
using RepositoryPatternWithUnitOfWork.Core.Interfaces;
using RepositoryPatternWithUnitOfWork.Core.InterfacesRepository;
using RepositoryPatternWithUnitOfWork.EF.Repositories;

namespace RepositoryPatternWithUnitOfWork.EF
{
    /// <summary>
    ///  Unit of Work Pattern
    /// - يوفر واجهة موحدة للوصول لكل الـ Repositories
    /// -  مسؤول عن SaveChanges (حفظ التغييرات)
    /// -  يضمن إن كل الـ Repositories بتستخدم نفس الـ DbContext
    /// - يدير الـ Transaction (كل العمليات تتحفظ مع بعض أو تفشل مع بعض)
    /// </summary>
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly ECommeeceDbContext _context;

        //  Backing fields nullable (lazy loading)
        private IUserRepository? _users;
        private IProductRepository? _products;
        private ICartProductRepository? _cartProducts;
        private ICartRepository? _carts;
        private ICategoryRepository? _categories;
        private IOrderItemRepository? _orderItems;
        private IOrderRepository? _orders;
        private IReviewRepository? _reviews;
        private IUserRoleRepository? _userRoles;
        private IPaymentRepository? _payment;
        private IProductImageRepository? _productImages;

        //  Constructor
        public UnitOfWork(ECommeeceDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        //  Lazy Loading Properties

        // المميزات: الـ Repository بيتعمل لما تحتاجه فقط

        public IUserRepository Users => _users ??= new UserRepository(_context);

        public IProductRepository Products => _products ??= new ProductRepository(_context);

        public ICartProductRepository CartProducts => _cartProducts ??= new CartProductRepository(_context) ;

        public ICartRepository Carts => _carts ??= new CartRepository(_context);

        public ICategoryRepository Categories => _categories ??= new CategoryRepository(_context);

        public IOrderItemRepository OrderItems => _orderItems ??= new OrderItemRepository(_context);

        public IOrderRepository Orders => _orders ??= new OrderRepository(_context);

        public IReviewRepository Reviews => _reviews ??= new ReviewRepository(_context);

        public IUserRoleRepository UserRoles => _userRoles ??= new UserRoleRepository(_context);

        public IPaymentRepository Payments => _payment ??= new PaymentRepository(_context);
        public IProductImageRepository ProductImages => _productImages ??= new ProductImageRepository(_context);




        //  SaveChanges موحد لكل العمليات

        public async Task<int> SaveChangesAsync() =>
            await _context.SaveChangesAsync();


        public int SaveChanges() =>
            _context.SaveChanges();

        //  Dispose Pattern
        public void Dispose()
        {
            _context?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}