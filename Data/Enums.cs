namespace Yoser_API.Data.Models
{
    public enum UserRole { Patient, Provider, Admin }

    // التمييز المطلوب في الصورة داخل رول المرضى
    public enum PatientCategory { Senior, Determination }

    // التمييز المطلوب في الصورة داخل رول مقدم الخدمة
    public enum ProviderType { Doctor, Nurse }
}