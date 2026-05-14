from fastapi import FastAPI
from pydantic import BaseModel
from openai import OpenAI
import json
import os

app = FastAPI()

# OPENROUTER CLIENT
client = OpenAI(
    api_key=os.getenv("OPENROUTER_API_KEY"),
    base_url="https://openrouter.ai/api/v1"
)

# REQUEST MODEL
class TextIn(BaseModel):
    text: str

# API ENDPOINT
@app.post("/analyze_feedback")
def analyze_feedback(payload: TextIn):

    try:

        prompt = f"""
You are an API.

You MUST return ONLY valid JSON.

No explanation.
No markdown.
No headings.
No extra text.

Analyze this customer feedback:

"{payload.text}"

Return EXACTLY this format:

{{
  "summary": "short summary",
  "sentiment": "POSITIVE or NEGATIVE or NEUTRAL",
  "category": "issue category",
  "recommendedAction": "recommended fix"
}}

ONLY RETURN JSON.
"""

        response = client.chat.completions.create(

            model="meta-llama/llama-3-8b-instruct",

            messages=[
                {
                    "role": "user",
                    "content": prompt
                }
            ],

            temperature=0.2
        )

        print("FULL RESPONSE:")
        print(response)

        text = response.choices[0].message.content

        print("RAW TEXT:")
        print(text)

        # CLEAN RESPONSE
        text = text.replace("```json", "")
        text = text.replace("```", "")
        text = text.strip()

        # EXTRACT JSON ONLY
        start = text.find("{")
        end = text.rfind("}") + 1

        if start != -1 and end != -1:
            text = text[start:end]

        print("CLEANED JSON:")
        print(text)

        # PARSE JSON
        result = json.loads(text)

        return result

    except Exception as e:

        return {
            "api_error": str(e)
        }