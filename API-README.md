# CompSci API Documentation

## Base URL

| Environment | URL |
|---|---|
| HTTP | `http://localhost:5294` |
| HTTPS | `https://localhost:7009` |

All endpoints are prefixed with `/api`. Swagger UI available at `/swagger` in development.

---

## Authentication

**Mechanism:** JWT Bearer Token (HS256, 24h expiry)

Include the token in every authenticated request:

```
Authorization: Bearer <token>
```

Obtain a token via `POST /api/Auth/login` or `POST /api/Auth/register`.

### JWT Claims

| Claim | Description |
|---|---|
| `sub` | User ID (Guid) |
| `email` | User email |
| `uniqueName` | Username |
| `role` | User role (Admin, Lecturer, Student) |
| `jti` | Token ID |

---

## CORS

All origins, methods, and headers are allowed (`AllowAll` policy).

---

## Common Response Formats

### Standard Response Envelope

Every endpoint returns data wrapped in this structure:

```json
{
  "success": true,
  "message": "string",
  "data": { ... },
  "errors": null
}
```

| Field | Type | Description |
|---|---|---|
| `success` | `boolean` | Whether the request succeeded |
| `message` | `string` | Human-readable status message |
| `data` | `T \| null` | Response payload (type varies per endpoint) |
| `errors` | `string[] \| null` | Validation or server errors |

### Paginated Response Envelope

Endpoints returning lists with pagination use this structure inside `data`:

```json
{
  "data": [ ... ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalRecords": 100,
  "totalPages": 10
}
```

---

## Roles

| Role | Value |
|---|---|
| Admin | `0` |
| Lecturer | `1` |
| Student | `2` |

---

## Error Handling

Unhandled exceptions are caught by middleware and returned as:

```json
{
  "success": false,
  "message": "Error message",
  "data": null,
  "errors": ["Detailed error"]
}
```

| HTTP Status | Meaning |
|---|---|
| 400 | Validation error / bad request |
| 401 | Missing or invalid JWT token |
| 403 | Insufficient role permissions |
| 404 | Resource not found |
| 500 | Internal server error |

---

## Endpoints

---

### Authentication

#### Register

```
POST /api/Auth/register
```

**Auth:** None (public)

**Request Body:**

```json
{
  "username": "johndoe",
  "email": "john@example.com",
  "password": "Password1",
  "role": 2
}
```

| Field | Type | Required | Constraints |
|---|---|---|---|
| `username` | `string` | Yes | 3-50 characters |
| `email` | `string` | Yes | Valid email format |
| `password` | `string` | Yes | Min 8 chars, must contain uppercase, lowercase, and digit |
| `role` | `int` | No | `0`=Admin, `1`=Lecturer, `2`=Student (default: Student) |

**Response (201 Created):**

```json
{
  "success": true,
  "message": "User registered successfully",
  "data": {
    "id": "a1b2c3d4-...",
    "username": "johndoe",
    "email": "john@example.com",
    "role": "Student",
    "token": "eyJhbGciOi...",
    "tokenExpiration": "2026-03-31T11:00:00Z"
  },
  "errors": null
}
```

---

#### Login

```
POST /api/Auth/login
```

**Auth:** None (public)

**Request Body:**

```json
{
  "email": "john@example.com",
  "password": "Password1"
}
```

| Field | Type | Required |
|---|---|---|
| `email` | `string` | Yes |
| `password` | `string` | Yes |

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "id": "a1b2c3d4-...",
    "username": "johndoe",
    "email": "john@example.com",
    "role": "Student",
    "token": "eyJhbGciOi...",
    "tokenExpiration": "2026-03-31T11:00:00Z"
  },
  "errors": null
}
```

---

#### Get User by ID

```
GET /api/Auth/users/{id}
```

**Auth:** Admin only

**Path Parameters:**

| Param | Type | Description |
|---|---|---|
| `id` | `Guid` | User ID |

**Response (200 OK):**

```json
{
  "success": true,
  "message": "User retrieved",
  "data": {
    "id": "a1b2c3d4-...",
    "username": "johndoe",
    "email": "john@example.com",
    "role": "Student",
    "createdAt": "2026-01-15T08:00:00Z"
  },
  "errors": null
}
```

---

#### Get All Users

```
GET /api/Auth/users
```

**Auth:** Admin only

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Users retrieved",
  "data": [
    {
      "id": "a1b2c3d4-...",
      "username": "johndoe",
      "email": "john@example.com",
      "role": "Student",
      "createdAt": "2026-01-15T08:00:00Z"
    }
  ],
  "errors": null
}
```

---

### Students

#### Create Student

```
POST /api/Students
```

**Auth:** Admin, Lecturer

**Request Body:**

```json
{
  "firstName": "John",
  "lastName": "Doe",
  "studentId": "STU001",
  "programName": "Computer Science",
  "year": 3,
  "enrollmentYear": 2024,
  "expectedGraduation": 2027
}
```

| Field | Type | Required | Constraints |
|---|---|---|---|
| `firstName` | `string` | Yes | Max 100 characters |
| `lastName` | `string` | Yes | Max 100 characters |
| `studentId` | `string` | Yes | Max 20 characters |
| `programName` | `string` | Yes | - |
| `year` | `int` | Yes | 1-10 |
| `enrollmentYear` | `int` | Yes | 1900 to current year + 1 |
| `expectedGraduation` | `int` | Yes | Must be >= enrollmentYear |

**Response (201 Created):**

```json
{
  "success": true,
  "message": "Student created",
  "data": {
    "id": "a1b2c3d4-...",
    "firstName": "John",
    "lastName": "Doe",
    "fullName": "John Doe",
    "studentId": "STU001",
    "programName": "Computer Science",
    "year": 3,
    "enrollmentYear": 2024,
    "expectedGraduation": 2027,
    "createdAt": "2026-03-30T11:00:00Z",
    "updatedAt": null
  },
  "errors": null
}
```

---

#### Get All Students

```
GET /api/Students
```

**Auth:** JWT required

**Response (200 OK):** `ApiResponse<IEnumerable<StudentResponse>>`

---

#### Get Students (Paginated)

```
GET /api/Students/paged?pageNumber=1&pageSize=10
```

**Auth:** JWT required

**Query Parameters:**

| Param | Type | Default | Description |
|---|---|---|---|
| `pageNumber` | `int` | 1 | Page number |
| `pageSize` | `int` | 10 | Items per page |

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Students retrieved",
  "data": {
    "data": [ ... ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalRecords": 50,
    "totalPages": 5
  },
  "errors": null
}
```

---

#### Get Student by ID

```
GET /api/Students/{id}
```

**Auth:** JWT required

**Path Parameters:**

| Param | Type |
|---|---|
| `id` | `Guid` |

**Response (200 OK):** `ApiResponse<StudentResponse>`

---

#### Update Student

```
PUT /api/Students/{id}
```

**Auth:** Admin, Lecturer

**Path Parameters:**

| Param | Type |
|---|---|
| `id` | `Guid` |

**Request Body:** Same as Create Student

**Response (200 OK):** `ApiResponse<StudentResponse>`

---

#### Delete Student

```
DELETE /api/Students/{id}
```

**Auth:** Admin only

**Path Parameters:**

| Param | Type |
|---|---|
| `id` | `Guid` |

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Student deleted",
  "data": true,
  "errors": null
}
```

---

### Courses

#### Create Course

```
POST /api/Courses
```

**Auth:** Admin, Lecturer

**Request Body:**

```json
{
  "courseCode": "CS101",
  "courseName": "Introduction to Programming",
  "creditHour": 3,
  "staff": "Dr. Smith"
}
```

| Field | Type | Required | Constraints |
|---|---|---|---|
| `courseCode` | `string` | Yes | Max 20 characters |
| `courseName` | `string` | Yes | Max 200 characters |
| `creditHour` | `int` | Yes | 1-10 |
| `staff` | `string` | Yes | - |

**Response (201 Created):**

```json
{
  "success": true,
  "message": "Course created",
  "data": {
    "id": "a1b2c3d4-...",
    "courseCode": "CS101",
    "courseName": "Introduction to Programming",
    "creditHour": 3,
    "staff": "Dr. Smith",
    "createdAt": "2026-03-30T11:00:00Z",
    "updatedAt": null
  },
  "errors": null
}
```

---

#### Get All Courses

```
GET /api/Courses
```

**Auth:** JWT required

**Response (200 OK):** `ApiResponse<IEnumerable<CourseResponse>>`

---

#### Get Courses (Paginated)

```
GET /api/Courses/paged?pageNumber=1&pageSize=10
```

**Auth:** JWT required

**Query Parameters:**

| Param | Type | Default |
|---|---|---|
| `pageNumber` | `int` | 1 |
| `pageSize` | `int` | 10 |

**Response (200 OK):** `ApiResponse<PagedResponse<CourseResponse>>`

---

#### Get Course by ID

```
GET /api/Courses/{id}
```

**Auth:** JWT required

**Path Parameters:**

| Param | Type |
|---|---|
| `id` | `Guid` |

**Response (200 OK):** `ApiResponse<CourseResponse>`

---

#### Update Course

```
PUT /api/Courses/{id}
```

**Auth:** Admin, Lecturer

**Path Parameters:**

| Param | Type |
|---|---|
| `id` | `Guid` |

**Request Body:** Same as Create Course

**Response (200 OK):** `ApiResponse<CourseResponse>`

---

#### Delete Course

```
DELETE /api/Courses/{id}
```

**Auth:** Admin only

**Path Parameters:**

| Param | Type |
|---|---|
| `id` | `Guid` |

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Course deleted",
  "data": true,
  "errors": null
}
```

---

### Assignments

#### Create Assignment

```
POST /api/Assignments
```

**Auth:** Admin, Lecturer

**Content-Type:** `multipart/form-data`

**Form Fields:**

| Field | Type | Required | Constraints |
|---|---|---|---|
| `courseName` | `string` | Yes | - |
| `courseCode` | `string` | Yes | - |
| `assignmentTitle` | `string` | Yes | Max 300 characters |
| `importance` | `int` | No | `0`=Low, `1`=Medium (default), `2`=High |
| `dueDate` | `DateTime` | Yes | Must not be in the past |
| `file` | `file` | Yes | PDF, DOCX, or DOC only (max 20MB) |

**Example (cURL):**

```bash
curl -X POST http://localhost:5294/api/Assignments \
  -H "Authorization: Bearer <token>" \
  -F "courseName=Introduction to Programming" \
  -F "courseCode=CS101" \
  -F "assignmentTitle=Final Project Submission" \
  -F "importance=2" \
  -F "dueDate=2026-04-15T23:59:00Z" \
  -F "file=@/path/to/assignment.pdf"
```

**Response (201 Created):**

```json
{
  "success": true,
  "message": "Assignment created successfully.",
  "data": {
    "id": "a1b2c3d4-...",
    "courseName": "Introduction to Programming",
    "courseCode": "CS101",
    "assignmentTitle": "Final Project Submission",
    "importance": 2,
    "importanceText": "High",
    "filePath": "uploads/assignments/...",
    "originalFileName": "assignment.pdf",
    "dateCreated": "2026-03-30T11:00:00Z",
    "dueDate": "2026-04-15T23:59:00Z"
  },
  "errors": null
}
```

---

#### Get All Assignments

```
GET /api/Assignments
```

**Auth:** JWT required

**Response (200 OK):** `ApiResponse<IEnumerable<AssignmentResponse>>`

---

#### Get Assignments (Paginated)

```
GET /api/Assignments/paged?pageNumber=1&pageSize=10
```

**Auth:** JWT required

**Query Parameters:**

| Param | Type | Default |
|---|---|---|
| `pageNumber` | `int` | 1 |
| `pageSize` | `int` | 10 |

**Response (200 OK):** `ApiResponse<PagedResponse<AssignmentResponse>>`

---

#### Get Assignment by ID

```
GET /api/Assignments/{id}
```

**Auth:** JWT required

**Path Parameters:**

| Param | Type |
|---|---|
| `id` | `Guid` |

**Response (200 OK):** `ApiResponse<AssignmentResponse>`

---

#### Download Assignment File

```
GET /api/Assignments/{id}/download
```

**Auth:** JWT required

**Path Parameters:**

| Param | Type |
|---|---|
| `id` | `Guid` |

**Response:** Binary file stream with original filename. Use `response.blob()` in JavaScript to handle.

---

#### Update Assignment

```
PUT /api/Assignments/{id}
```

**Auth:** Admin, Lecturer

**Content-Type:** `multipart/form-data`

**Path Parameters:**

| Param | Type |
|---|---|
| `id` | `Guid` |

**Form Fields:**

| Field | Type | Required |
|---|---|---|
| `courseName` | `string` | Yes |
| `courseCode` | `string` | Yes |
| `assignmentTitle` | `string` | Yes |
| `importance` | `int` | Yes |
| `dueDate` | `DateTime` | Yes |
| `file` | `file` | No (optional replacement) |

**Response (200 OK):** `ApiResponse<AssignmentResponse>`

---

#### Delete Assignment

```
DELETE /api/Assignments/{id}
```

**Auth:** Admin, Lecturer

**Path Parameters:**

| Param | Type |
|---|---|
| `id` | `Guid` |

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Assignment deleted",
  "data": true,
  "errors": null
}
```

---

### Notes

#### Upload Note

```
POST /api/Notes
```

**Auth:** Admin, Lecturer, Student

**Content-Type:** `multipart/form-data`

**Form Fields:**

| Field | Type | Required | Constraints |
|---|---|---|---|
| `courseName` | `string` | Yes | - |
| `courseCode` | `string` | Yes | - |
| `file` | `file` | Yes | PDF or DOCX only |

**Example (cURL):**

```bash
curl -X POST http://localhost:5294/api/Notes \
  -H "Authorization: Bearer <token>" \
  -F "courseName=Introduction to Programming" \
  -F "courseCode=CS101" \
  -F "file=@/path/to/lecture_notes.pdf"
```

**Response (201 Created):**

```json
{
  "success": true,
  "message": "Note uploaded",
  "data": {
    "id": "a1b2c3d4-...",
    "courseName": "Introduction to Programming",
    "courseCode": "CS101",
    "filePath": "/uploads/notes/...",
    "originalFileName": "lecture_notes.pdf",
    "uploadDate": "2026-03-30T11:00:00Z"
  },
  "errors": null
}
```

---

#### Get All Notes

```
GET /api/Notes
```

**Auth:** JWT required

**Response (200 OK):** `ApiResponse<IEnumerable<NoteResponse>>`

---

#### Get Notes (Paginated)

```
GET /api/Notes/paged?pageNumber=1&pageSize=10
```

**Auth:** JWT required

**Query Parameters:**

| Param | Type | Default |
|---|---|---|
| `pageNumber` | `int` | 1 |
| `pageSize` | `int` | 10 |

**Response (200 OK):** `ApiResponse<PagedResponse<NoteResponse>>`

---

#### Get Note by ID

```
GET /api/Notes/{id}
```

**Auth:** JWT required

**Path Parameters:**

| Param | Type |
|---|---|
| `id` | `Guid` |

**Response (200 OK):** `ApiResponse<NoteResponse>`

---

#### Download Note File

```
GET /api/Notes/{id}/download
```

**Auth:** JWT required

**Path Parameters:**

| Param | Type |
|---|---|
| `id` | `Guid` |

**Response:** Binary file stream with original filename. Use `response.blob()` in JavaScript to handle.

---

#### Update Note

```
PUT /api/Notes/{id}
```

**Auth:** Admin, Lecturer

**Content-Type:** `multipart/form-data`

**Path Parameters:**

| Param | Type |
|---|---|
| `id` | `Guid` |

**Form Fields:**

| Field | Type | Required |
|---|---|---|
| `courseName` | `string` | Yes |
| `courseCode` | `string` | Yes |
| `file` | `file` | No (optional replacement) |

**Response (200 OK):** `ApiResponse<NoteResponse>`

---

#### Delete Note

```
DELETE /api/Notes/{id}
```

**Auth:** Admin, Lecturer

**Path Parameters:**

| Param | Type |
|---|---|
| `id` | `Guid` |

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Note deleted",
  "data": true,
  "errors": null
}
```

---

### Past Questions

#### Upload Past Question

```
POST /api/PastQuestions
```

**Auth:** Admin, Lecturer

**Content-Type:** `multipart/form-data`

**Form Fields:**

| Field | Type | Required | Constraints |
|---|---|---|---|
| `courseName` | `string` | Yes | - |
| `courseCode` | `string` | Yes | - |
| `file` | `file` | Yes | PDF only |

**Example (cURL):**

```bash
curl -X POST http://localhost:5294/api/PastQuestions \
  -H "Authorization: Bearer <token>" \
  -F "courseName=Introduction to Programming" \
  -F "courseCode=CS101" \
  -F "file=@/path/to/past_questions.pdf"
```

**Response (201 Created):**

```json
{
  "success": true,
  "message": "Past question uploaded",
  "data": {
    "id": "a1b2c3d4-...",
    "courseName": "Introduction to Programming",
    "courseCode": "CS101",
    "filePath": "/uploads/pastquestions/...",
    "originalFileName": "past_questions.pdf",
    "uploadDate": "2026-03-30T11:00:00Z"
  },
  "errors": null
}
```

---

#### Get All Past Questions

```
GET /api/PastQuestions
```

**Auth:** JWT required

**Response (200 OK):** `ApiResponse<IEnumerable<PastQuestionResponse>>`

---

#### Get Past Questions (Paginated)

```
GET /api/PastQuestions/paged?pageNumber=1&pageSize=10
```

**Auth:** JWT required

**Query Parameters:**

| Param | Type | Default |
|---|---|---|
| `pageNumber` | `int` | 1 |
| `pageSize` | `int` | 10 |

**Response (200 OK):** `ApiResponse<PagedResponse<PastQuestionResponse>>`

---

#### Get Past Question by ID

```
GET /api/PastQuestions/{id}
```

**Auth:** JWT required

**Path Parameters:**

| Param | Type |
|---|---|
| `id` | `Guid` |

**Response (200 OK):** `ApiResponse<PastQuestionResponse>`

---

#### Download Past Question File

```
GET /api/PastQuestions/{id}/download
```

**Auth:** JWT required

**Path Parameters:**

| Param | Type |
|---|---|
| `id` | `Guid` |

**Response:** Binary file stream with original filename. Use `response.blob()` in JavaScript to handle.

---

#### Update Past Question

```
PUT /api/PastQuestions/{id}
```

**Auth:** Admin, Lecturer

**Content-Type:** `multipart/form-data`

**Path Parameters:**

| Param | Type |
|---|---|
| `id` | `Guid` |

**Form Fields:**

| Field | Type | Required |
|---|---|---|
| `courseName` | `string` | Yes |
| `courseCode` | `string` | Yes |
| `file` | `file` | No (optional replacement) |

**Response (200 OK):** `ApiResponse<PastQuestionResponse>`

---

#### Delete Past Question

```
DELETE /api/PastQuestions/{id}
```

**Auth:** Admin, Lecturer

**Path Parameters:**

| Param | Type |
|---|---|
| `id` | `Guid` |

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Past question deleted",
  "data": true,
  "errors": null
}
```

---

## Frontend Integration Quick Start

### 1. Configure Axios (or fetch) base instance

```javascript
import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:5294/api',
  headers: { 'Content-Type': 'application/json' }
});

// Attach JWT to every request
api.interceptors.request.use(config => {
  const token = localStorage.getItem('token');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});
```

### 2. Login flow

```javascript
const login = async (email, password) => {
  const { data } = await api.post('/Auth/login', { email, password });
  // data.data contains { id, username, email, role, token, tokenExpiration }
  localStorage.setItem('token', data.data.token);
  localStorage.setItem('user', JSON.stringify(data.data));
  return data.data;
};
```

### 3. File upload (Notes / Past Questions / Assignments)

```javascript
const uploadNote = async (courseName, courseCode, file) => {
  const formData = new FormData();
  formData.append('courseName', courseName);
  formData.append('courseCode', courseCode);
  formData.append('file', file);

  const { data } = await api.post('/Notes', formData, {
    headers: { 'Content-Type': 'multipart/form-data' }
  });
  return data.data;
};

const uploadAssignment = async (courseName, courseCode, assignmentTitle, importance, dueDate, file) => {
  const formData = new FormData();
  formData.append('courseName', courseName);
  formData.append('courseCode', courseCode);
  formData.append('assignmentTitle', assignmentTitle);
  formData.append('importance', importance);
  formData.append('dueDate', dueDate);
  formData.append('file', file);

  const { data } = await api.post('/Assignments', formData, {
    headers: { 'Content-Type': 'multipart/form-data' }
  });
  return data.data;
};
```

### 4. File download

```javascript
const downloadNote = async (id) => {
  const response = await api.get(`/Notes/${id}/download`, {
    responseType: 'blob'
  });
  const url = window.URL.createObjectURL(new Blob([response.data]));
  const link = document.createElement('a');
  link.href = url;
  link.setAttribute('download', 'note.pdf');
  document.body.appendChild(link);
  link.click();
};

const downloadAssignment = async (id) => {
  const response = await api.get(`/Assignments/${id}/download`, {
    responseType: 'blob'
  });
  const url = window.URL.createObjectURL(new Blob([response.data]));
  const link = document.createElement('a');
  link.href = url;
  link.setAttribute('download', 'assignment.pdf');
  document.body.appendChild(link);
  link.click();
};
```

### 5. Paginated request

```javascript
const getStudents = async (pageNumber = 1, pageSize = 10) => {
  const { data } = await api.get('/Students/paged', {
    params: { pageNumber, pageSize }
  });
  // data.data = { data: [...], pageNumber, pageSize, totalRecords, totalPages }
  return data.data;
};
```

---

## Endpoint Summary

| # | Method | Endpoint | Auth | Description |
|---|---|---|---|---|
| 1 | POST | `/api/Auth/register` | Public | Register new user |
| 2 | POST | `/api/Auth/login` | Public | Login and receive JWT |
| 3 | GET | `/api/Auth/users/{id}` | Admin | Get user by ID |
| 4 | GET | `/api/Auth/users` | Admin | List all users |
| 5 | POST | `/api/Students` | Admin, Lecturer | Create student |
| 6 | GET | `/api/Students` | JWT | List all students |
| 7 | GET | `/api/Students/paged` | JWT | List students (paginated) |
| 8 | GET | `/api/Students/{id}` | JWT | Get student by ID |
| 9 | PUT | `/api/Students/{id}` | Admin, Lecturer | Update student |
| 10 | DELETE | `/api/Students/{id}` | Admin | Delete student |
| 11 | POST | `/api/Courses` | Admin, Lecturer | Create course |
| 12 | GET | `/api/Courses` | JWT | List all courses |
| 13 | GET | `/api/Courses/paged` | JWT | List courses (paginated) |
| 14 | GET | `/api/Courses/{id}` | JWT | Get course by ID |
| 15 | PUT | `/api/Courses/{id}` | Admin, Lecturer | Update course |
| 16 | DELETE | `/api/Courses/{id}` | Admin | Delete course |
| 17 | POST | `/api/Assignments` | Admin, Lecturer | Create assignment (PDF/DOCX) |
| 18 | GET | `/api/Assignments` | JWT | List all assignments |
| 19 | GET | `/api/Assignments/paged` | JWT | List assignments (paginated) |
| 20 | GET | `/api/Assignments/{id}` | JWT | Get assignment by ID |
| 21 | GET | `/api/Assignments/{id}/download` | JWT | Download assignment file |
| 22 | PUT | `/api/Assignments/{id}` | Admin, Lecturer | Update assignment |
| 23 | DELETE | `/api/Assignments/{id}` | Admin, Lecturer | Delete assignment |
| 23 | POST | `/api/Notes` | Admin, Lecturer, Student | Upload note (PDF/DOCX) |
| 24 | GET | `/api/Notes` | JWT | List all notes |
| 25 | GET | `/api/Notes/paged` | JWT | List notes (paginated) |
| 26 | GET | `/api/Notes/{id}` | JWT | Get note by ID |
| 27 | GET | `/api/Notes/{id}/download` | JWT | Download note file |
| 28 | PUT | `/api/Notes/{id}` | Admin, Lecturer | Update note |
| 29 | DELETE | `/api/Notes/{id}` | Admin, Lecturer | Delete note |
| 30 | POST | `/api/PastQuestions` | Admin, Lecturer | Upload past question (PDF) |
| 31 | GET | `/api/PastQuestions` | JWT | List all past questions |
| 32 | GET | `/api/PastQuestions/paged` | JWT | List past questions (paginated) |
| 33 | GET | `/api/PastQuestions/{id}` | JWT | Get past question by ID |
| 34 | GET | `/api/PastQuestions/{id}/download` | JWT | Download past question file |
| 35 | PUT | `/api/PastQuestions/{id}` | Admin, Lecturer | Update past question |
| 36 | DELETE | `/api/PastQuestions/{id}` | Admin, Lecturer | Delete past question |
