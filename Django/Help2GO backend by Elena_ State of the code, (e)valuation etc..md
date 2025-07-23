**Help2GO backend** by Elena: **State of the code**, (e)valuation etc.

BASIC INFO: 

**Django-based project** (presence of `manage.py`, `settings.py`).  
**Database**: SQLite (`db.sqlite3`).  
**Deployment**: Includes `procfile`, `runtime.txt`, and a **GitHub Actions workflow (`.github/workflows/run_cron.yaml`)**, suggesting **CI/CD automation**.  
**Main configuration files**: `backend/settings.py`, `backend/urls.py`, `backend/wsgi.py`.  
**Dependencies**: `requirements.txt`.

CODE ANALYSIS:

#### **1\. Overview**

* A **Django 5.x project**.  
* Uses **PostgreSQL** (instead of SQLite) with `dj-database-url`.  
* **JWT authentication** for API security.  
* **CORS handling** similar to the first backend.  
* **Includes AWS integration** (for S3 storage).  
* **Has cron jobs** for scheduled tasks (`django-crontab`).  
* **Uses CI/CD** via GitHub Actions (`.github/workflows/run_cron.yaml`).

---

#### **2\. Structure & Key Components**

* **Main Apps Installed**:  
  * `events` (likely handles event-related data)  
  * `user` (manages user authentication and profiles)  
  * `shared` (general utilities)  
  * `ngo` (specific to NGO-related data)  
  * `django_crontab` (for scheduled tasks)  
  * `rest_framework`, `corsheaders`, `rest_framework_simplejwt`, `whitenoise`  
* **New Middleware Additions**:  
  * Same as the first backend but with `django_crontab`.  
* **Authentication & Security**:  
  * Uses **JWT-based authentication**.  
  * **CORS is still open to all** (`CORS_ALLOW_ALL_ORIGINS=True`).  
  * Custom user model (`AUTH_USER_MODEL = 'user.CustomUser'`).  
  * Includes **AWS settings** for handling file storage.  
* **Routing**:  
  * `admin/` → Django Admin Panel.  
  * Default (`''`) → Routes to `events`, `user`, `ngo`, and `shared` apps.  
* **Database**:  
  * Uses **PostgreSQL** via `dj-database-url`, which is better for production.  
* **Cron Jobs**:

Automatically resets streaks daily via:  
python  
KopirajUredi  
`CRONJOBS = [`  
    `('0 0 * * *', 'user.cron.reset_streaks'),`  
`]`

*   
* **AWS S3 Integration**:

Environment variables are loaded for AWS storage:  
python  
KopirajUredi  
`AWS_ACCESS_KEY_ID = os.environ.get("AWS_ACCESS_KEY_ID")`  
`AWS_SECRET_ACCESS_KEY = os.environ.get("AWS_SECRET_ACCESS_KEY")`  
`AWS_STORAGE_BUCKET_NAME = os.environ.get("AWS_STORAGE_BUCKET_NAME")`  
`AWS_S3_REGION_NAME = os.environ.get("AWS_S3_REGION_NAME")`

*   
* **Email Configuration**:

Uses SMTP for email sending:  
python  
KopirajUredi  
`EMAIL_BACKEND = 'django.core.mail.backends.smtp.EmailBackend'`  
`EMAIL_HOST = os.environ.get("EMAIL_HOST")`  
`EMAIL_USE_TLS = True`  
`EMAIL_PORT = 587`  
`EMAIL_HOST_USER = os.environ.get("EMAIL_HOST_USER")`  
`EMAIL_HOST_PASSWORD = os.environ.get("EMAIL_HOST_PASSWORD")`

* 

---

#### **3\. Dependencies (requirements.txt)**

* **Core**:  
  * `Django==5.1.6` (newer than the first backend)  
  * `djangorestframework`  
* **Security & Auth**:  
  * `djangorestframework-simplejwt`  
  * `python-dotenv`  
* **CORS Support**:  
  * `django-cors-headers`  
* **Database**:  
  * `dj-database-url` (for PostgreSQL database configuration)  
  * `psycopg2-binary` (PostgreSQL driver)  
* **Task Scheduling**:  
  * `django-crontab`  
* **AWS S3 Storage**:  
  * `boto3`, `botocore`, `django-storages`  
* **Deployment Tools**:  
  * `gunicorn`, `whitenoise`

---

#### **4\. Potential Issues**

* **Security Concerns**:  
  * **CORS is open to all** (`CORS_ALLOW_ALL_ORIGINS=True`), which should be restricted.  
  * **DEBUG \= True**, which should be `False` in production.  
  * **AWS credentials should be securely stored** in `.env`.  
* **Database Choice**:  
  * **Uses PostgreSQL** (better for production than SQLite).  
* **Cron Jobs Execution**:  
  * If the hosting provider does not support `cron`, this feature might fail.

---

### **Conclusion**

* **Strengths**:  
  * **More production-ready**: Uses **PostgreSQL** instead of SQLite.  
  * **AWS S3 integration**: Handles file storage efficiently.  
  * **Cron jobs**: Automates background tasks like resetting user streaks.  
  * **More updated Django version** (`5.1.6` vs `5.0.6` in the first backend).  
* **Areas for Improvement**:  
  * **Security**: CORS settings need restriction.  
  * **DEBUG setting should be False** in production.  
  * **Ensure AWS credentials are properly secured**.

COMPARISON TO THE PROTOTYPE CODE:

### **Comparison of the Two Backends**

Both projects share a similar structure, confirming that the second codebase is an evolution of the first. Below is a detailed comparison:

| Feature | First Backend (Older) | Second Backend (Newer) | Changes & Improvements |
| ----- | ----- | ----- | ----- |
| **Framework** | Django 5.0.6 | Django 5.1.6 | **Upgraded Django version** |
| **Database** | SQLite (`db.sqlite3`) | PostgreSQL (`dj-database-url`) | **PostgreSQL is more production-ready** |
| **Authentication** | JWT-based auth | JWT-based auth | No change |
| **CORS Handling** | `CORS_ALLOW_ALL_ORIGINS=True` | `CORS_ALLOW_ALL_ORIGINS=True` | Needs restriction in both versions |
| **Apps Installed** | `events`, `user`, `shared`, `ngo` | `events`, `user`, `shared`, `ngo` | Same |
| **Task Scheduling** | No task scheduling | `django-crontab` | **New cron job feature (e.g., resetting user streaks)** |
| **File Storage** | Local (default Django) | AWS S3 (`boto3`, `django-storages`) | **AWS integration for file storage** |
| **Email Support** | None configured | SMTP-based email sending | **Added SMTP email configuration** |
| **Deployment Tools** | `whitenoise`, `gunicorn` | `whitenoise`, `gunicorn`, **GitHub Actions** | **Added CI/CD support** |
| **Security** | `.env` used for `SECRET_KEY` but SQLite is insecure | `.env` for `SECRET_KEY` \+ AWS credentials in env \+ PostgreSQL | **More security considerations in newer version** |
| **CSRF Trusted Origins** | Predefined Render & Railway URLs | Same \+ `127.0.0.1` (local dev support) | **Added localhost support** |
| **DEBUG Mode** | `True` (should be False in production) | `True` (should be False in production) | **No improvement—still a security risk** |

PRICING VALUATION:

#### **1\. Key Considerations for Pricing Evaluation**

1. **Complexity & Scope** – The features implemented and their complexity.  
2. **Code Quality & Best Practices** – Clean code, maintainability, security, scalability.  
3. **Time & Effort** – Estimated hours of work, debugging, and testing.  
4. **Industry Benchmark** – What similar work typically costs in the market.  
5. **Improvements from Previous Versions** – If the second version is truly worth the extra cost.

### **2\. Breakdown of the Prototype Code**

#### **What Was Delivered:**

✅ Basic Django backend setup  
✅ API endpoints for multiple apps (`events`, `user`, `ngo`, `shared`)  
✅ JWT authentication  
✅ Basic SQLite database  
✅ Deployment setup (`whitenoise`, `gunicorn`)  
✅ Open CORS configuration (could be a security flaw)

#### **Estimated Effort:**

* Setting up a Django REST framework: **\~20-30 hours** (junior dev)  
* API logic & implementation: **\~15-25 hours**  
* Debugging & testing: **\~10-15 hours**  
* Deployment & environment configuration: **\~5-10 hours**  
* **Total Estimated Work: \~50-70 hours**

#### **Price Justification (650 EUR)**

* **Junior Developer Rate (\~10-15 EUR/hr)**: 50 hours × **13 EUR/hr** ≈ **650 EUR** ✅  
* Reasonable for a junior developer **considering a functional but not optimized backend**.  
* **Evaluation: Fair pricing** for what was delivered.

---

### **3\. Breakdown of the MVP (new) Code (Requested 1,000 EUR)**

#### **New Features & Enhancements:**

✅ **Upgraded database to PostgreSQL** (more scalable)  
✅ **AWS S3 storage support** (file management improvement)  
✅ **Task automation with `django-crontab`**  
✅ **Email functionality via SMTP**  
✅ **CI/CD setup (GitHub Actions for auto deployment)**  
✅ **Security improvements** (though `DEBUG=True` & `CORS_ALLOW_ALL_ORIGINS=True` still present)

#### **Estimated Effort:**

* Upgrading from SQLite to PostgreSQL: **\~5-10 hours**  
* Setting up AWS S3 & file storage: **\~10-15 hours**  
* Adding cron jobs & scheduling: **\~5-8 hours**  
* Implementing email system: **\~5-8 hours**  
* CI/CD pipeline (GitHub Actions): **\~10-15 hours**  
* Debugging, testing & refactoring: **\~10-15 hours**  
* **Total Estimated Work: \~50-70 additional hours**

#### **Price Justification (1,000 EUR)**

* **Junior Developer Rate (\~15-20 EUR/hr)**: 50 hours × **13 EUR/hr** ≈ **650 EUR**  
* Since this is **an improvement on an existing project**, the price should reflect effort but also **reuse of previous work**.

