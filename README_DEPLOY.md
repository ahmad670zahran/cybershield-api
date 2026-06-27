# 🚀 CyberShield API — جاهز للـ Deploy على Railway

## خطوتين بس:

### 1️⃣ ارفع الفولدر ده على GitHub
```bash
git init
git add .
git commit -m "Ready for Railway deploy"
git remote add origin https://github.com/YOUR_USERNAME/cybershield-api.git
git push -u origin main
```

### 2️⃣ ادخل Railway وربطه
1. روح [railway.app](https://railway.app)
2. **New Project → Deploy from GitHub repo**
3. اختر الـ repo
4. Railway هيبني تلقائياً من الـ `Dockerfile` ✅

### بعد الـ Deploy:
- الـ URL هيبقى زي: `https://cybershield-api-xyz.up.railway.app`
- الـ Swagger: `https://cybershield-api-xyz.up.railway.app/swagger`
- الـ Health: `https://cybershield-api-xyz.up.railway.app/health`

### Admin Login الافتراضي:
- Email: `superadmin@technocrypt.com`
- Password: `AdminSecure123!@#`

---
الـ DB بتتعمل تلقائياً عند أول تشغيل — مفيش حاجة تعملها يدوياً.
