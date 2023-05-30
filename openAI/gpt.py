import openai
from openai import GPT3Completion

class GPTIntegration:

    def __init__(self):
        openai.api_key = "your_openai_key"
        self.gpt = GPT3Completion()

    def get_ai_response(self, message):
        try:
            response = self.gpt.create(prompt=message, max_tokens=60)
            return response.choices[0].text.strip()
        except Exception as e:
            print(f"An error occurred: {e}")
            return None

gpt = GPTIntegration()

# Let's simulate a user input and get AI response
user_message = ""
ai_response = gpt.get_ai_response(user_message)

if ai_response:
    print(f"AI Response: {ai_response}")
else:
    print("No response received.")
