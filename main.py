# from fastapi import FastAPI, File, UploadFile, HTTPException
# from fastapi.responses import JSONResponse
# from pydantic import BaseModel
# import numpy as np
# import cv2
# import os
# from google.cloud import vision
# from sentence_transformers import SentenceTransformer, util
# from sklearn.feature_extraction.text import TfidfVectorizer
# from sklearn.metrics.pairwise import cosine_similarity
# import io
#
# # Set credentials
# os.environ["GOOGLE_APPLICATION_CREDENTIALS"] = r"D:\user\source\repos\OCR_Project\google-credentials.json"
#
# # Initialize FastAPI app
# app = FastAPI()
#
# # Load semantic model once
# semantic_model = SentenceTransformer('all-MiniLM-L6-v2')
#
# # Sample model answer (you could also load this from a DB or pass it as input)
# MODEL_ANSWER = """suddenly when you get
# new idea or insight you will see the world in different angle you going to express or implement new insight Happiness, in psychology
# state of emotion that a person experiences either in narrow Sense When good things happen"""
#
# TOTAL_MARKS = 10
#
# def preprocess_image_bytes(image_bytes: bytes):
#     np_arr = np.frombuffer(image_bytes, np.uint8)
#     img = cv2.imdecode(np_arr, cv2.IMREAD_COLOR)
#     gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
#     return gray
#
# def extract_text_from_image(image_np: np.ndarray):
#     client = vision.ImageAnnotatorClient()
#     success, encoded_image = cv2.imencode('.jpg', image_np)
#     if not success:
#         raise ValueError("Image encoding failed.")
#     image = vision.Image(content=encoded_image.tobytes())
#     response = client.document_text_detection(image=image)
#     if response.error.message:
#         raise Exception(f"Vision API error: {response.error.message}")
#     return response.full_text_annotation.text
#
# def semantic_similarity(model_answer, student_answer):
#     embeddings = semantic_model.encode([model_answer, student_answer], convert_to_tensor=True)
#     return util.cos_sim(embeddings[0], embeddings[1]).item()
#
# def tfidf_cosine_similarity(model_answer, student_answer):
#     vectors = TfidfVectorizer().fit_transform([model_answer, student_answer]).toarray()
#     return cosine_similarity([vectors[0]], [vectors[1]])[0][0]
#
# def grade(score, total):
#     return round(score * total, 2)
#
# @app.post("/evaluate")
# async def evaluate_image(file: UploadFile = File(...)):
#     try:
#         image_bytes = await file.read()
#         preprocessed_img = preprocess_image_bytes(image_bytes)
#         extracted_text = extract_text_from_image(preprocessed_img)
#
#         sem_score = semantic_similarity(MODEL_ANSWER, extracted_text)
#         tfidf_score = tfidf_cosine_similarity(MODEL_ANSWER, extracted_text)
#
#         return JSONResponse({
#             "extracted_text_preview": extracted_text[:300] + "...",
#             "semantic_score": round(sem_score, 2),
#             "semantic_grade": grade(sem_score, TOTAL_MARKS),
#             "tfidf_score": round(tfidf_score, 2),
#             "tfidf_grade": grade(tfidf_score, TOTAL_MARKS),
#         })
#     except Exception as e:
#         raise HTTPException(status_code=500, detail=str(e))
from fastapi import FastAPI, File, UploadFile, HTTPException
from fastapi.responses import JSONResponse
import numpy as np
import cv2
import os
from google.cloud import vision
from sentence_transformers import SentenceTransformer, util
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.metrics.pairwise import cosine_similarity
from io import BytesIO
from pdf2image import convert_from_bytes
from PIL import Image
import tempfile
from docx import Document

# Set credentials
os.environ["GOOGLE_APPLICATION_CREDENTIALS"] = r"D:\user\source\repos\OCR_Project\google-credentials.json"

app = FastAPI()
semantic_model = SentenceTransformer('all-MiniLM-L6-v2')
TOTAL_MARKS = 10

def preprocess_image_bytes(image_bytes: bytes):
    np_arr = np.frombuffer(image_bytes, np.uint8)
    img = cv2.imdecode(np_arr, cv2.IMREAD_COLOR)
    gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
    return gray

def extract_text_from_image(image_np: np.ndarray):
    client = vision.ImageAnnotatorClient()
    success, encoded_image = cv2.imencode('.jpg', image_np)
    if not success:
        raise ValueError("Image encoding failed.")
    image = vision.Image(content=encoded_image.tobytes())
    response = client.document_text_detection(image=image)
    if response.error.message:
        raise Exception(f"Vision API error: {response.error.message}")
    return response.full_text_annotation.text

def extract_text_from_docx(docx_bytes):
    with tempfile.NamedTemporaryFile(delete=False, suffix=".docx") as tmp_docx:
        tmp_docx.write(docx_bytes)
        tmp_docx.flush()
        doc = Document(tmp_docx.name)
        return "\n".join([p.text for p in doc.paragraphs])

def extract_text_pages_from_pdf(pdf_bytes):
    images = convert_from_bytes(pdf_bytes)
    all_text = []
    for img in images:
        buf = BytesIO()
        img.save(buf, format='JPEG')
        image_np = preprocess_image_bytes(buf.getvalue())
        page_text = extract_text_from_image(image_np)
        all_text.append(page_text)
    return all_text  # returns list of text strings, one per page

def semantic_similarity(model_answer, student_answer):
    embeddings = semantic_model.encode([model_answer, student_answer], convert_to_tensor=True)
    return util.cos_sim(embeddings[0], embeddings[1]).item()

def tfidf_cosine_similarity(model_answer, student_answer):
    vectors = TfidfVectorizer().fit_transform([model_answer, student_answer]).toarray()
    return cosine_similarity([vectors[0]], [vectors[1]])[0][0]

def grade(score, total):
    return round(score * total, 2)

@app.post("/evaluate")
async def evaluate_answer_sheet(
    student_file: UploadFile = File(...),
    model_file: UploadFile = File(...)
):
    try:
        student_ext = student_file.filename.split(".")[-1].lower()
        model_ext = model_file.filename.split(".")[-1].lower()

        student_content = await student_file.read()
        model_content = await model_file.read()

        # ---- Extract model answer ----
        if model_ext == "txt":
            model_answer = model_content.decode("utf-8")
        elif model_ext == "docx":
            model_answer = extract_text_from_docx(model_content)
        elif model_ext == "pdf":
            model_answer = "\n".join(extract_text_pages_from_pdf(model_content))
        else:
            raise HTTPException(status_code=400, detail="Unsupported model answer file type.")

        # ---- Extract student answer pages ----
        if student_ext == "pdf":
            student_pages = extract_text_pages_from_pdf(student_content)
        elif student_ext in ("jpg", "jpeg", "png"):
            # Treat as single image input
            image_np = preprocess_image_bytes(student_content)
            student_pages = [extract_text_from_image(image_np)]
        elif student_ext == "docx":
            student_pages = [extract_text_from_docx(student_content)]
        else:
            raise HTTPException(status_code=400, detail="Unsupported student file type.")

        # ---- Evaluate each page ----
        page_results = []
        for idx, page_text in enumerate(student_pages, start=1):
            sem_score = semantic_similarity(model_answer, page_text)
            tfidf_score = tfidf_cosine_similarity(model_answer, page_text)
            page_results.append({
                "page": idx,
                "semantic_score": round(sem_score, 2),
                "semantic_grade": grade(sem_score, TOTAL_MARKS),
                "tfidf_score": round(tfidf_score, 2),
                "tfidf_grade": grade(tfidf_score, TOTAL_MARKS),
                "page_text_preview": page_text[:250] + "..."
            })

        return JSONResponse({
            "model_text_preview": model_answer[:300] + "...",
            "pages_evaluated": len(student_pages),
            "page_results": page_results
        })

    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
