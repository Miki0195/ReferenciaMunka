"""Hungarian NLP utilities."""

import re
from typing import List

# Common Hungarian stopwords
HUNGARIAN_STOPWORDS = {"a", "az", "és", "is", "hogy", "nem", "egy"}

def clean_hungarian_text(text: str) -> str:
    """Clean Hungarian text."""
    paragraphs = text.split("\n\n")
    cleaned = []
    
    for p in paragraphs:
        p = p.strip()
        if not p:
            continue
        p = re.sub(r"\s+", " ", p)
        cleaned.append(p)
    
    return "\n\n".join(cleaned)

def fix_sentence_boundaries(sentences: List[str]) -> List[str]:
    """Fix sentence boundaries."""
    corrected = []
    
    for i, sentence in enumerate(sentences):
        s = sentence.strip()
        if not s:
            continue
            
        if i > 0 and s and s[0].islower() and len(s) < 50:
            corrected[-1] = corrected[-1] + " " + s
        else:
            corrected.append(s)
    
    return corrected

def get_hungarian_sentence_importance(sentence: str) -> float:
    """Calculate sentence importance."""
    score = 1.0
    words = sentence.split()
    
    if 5 <= len(words) <= 25:
        score += 0.5
    
    if re.search(r"\d", sentence):
        score += 0.3
        
    non_stopwords = [w for w in words if w.lower() not in HUNGARIAN_STOPWORDS]
    if words:
        score += 0.3 * (len(non_stopwords) / len(words))
    
    return score 