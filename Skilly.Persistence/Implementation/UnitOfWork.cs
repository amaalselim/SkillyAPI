using Skilly.Core.Entities;
using Skilly.Persistence.Abstract;
using Skilly.Persistence.DataContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skilly.Persistence.Implementation
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public IPaymentRepository _paymentRepository { get; private set; }

        public IGenericRepository<User> Users { get; private set; }

        public IUserProfileRepository ProfileRepository { get; private set; }

        public IServiceProviderRepository ServiceProviderRepository { get; private set; }

        public IServicegalleryRepository servicegalleryRepository { get; private set; }

        public IProviderServicesRepository providerServiceRepository {  get; private set; }

        public IReviewRepository reviewRepository {  get; private set; }
        public ICategoryRepository _categoryRepository {  get; private set; }
        public IRequestserviceRepository _requestserviceRepository {  get; private set; }
        public IOfferSalaryRepository _OfferSalaryRepository { get; private set; }

        public IBannerService _BannerService {  get; private set; }

        public UnitOfWork(IGenericRepository<User> User,
            ApplicationDbContext context,
            IUserProfileRepository userProfileRepository,
           IServiceProviderRepository serviceProviderRepository,
           IServicegalleryRepository ServicegalleryRepository,
           IProviderServicesRepository providerServicesRepository,
           IReviewRepository ReviewRepository,
           ICategoryRepository categoryRepository,
           IRequestserviceRepository requestserviceRepository,
           IOfferSalaryRepository OfferSalaryRepository,
           IBannerService bannerService,
           IPaymentRepository paymentRepository

           )
        {
            _context = context;
            Users = User;
            ProfileRepository = userProfileRepository;
            ServiceProviderRepository = serviceProviderRepository;
            servicegalleryRepository = ServicegalleryRepository;
            providerServiceRepository = providerServicesRepository;
            reviewRepository = ReviewRepository;
            _categoryRepository = categoryRepository;
            _requestserviceRepository = requestserviceRepository;
            _OfferSalaryRepository = OfferSalaryRepository;
            _BannerService = bannerService;
            _paymentRepository = paymentRepository;
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
