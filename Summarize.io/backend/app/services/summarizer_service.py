from typing import Tuple, List
import re
import logging
from functools import lru_cache

# Import optional dependencies with try/except
try:
    import torch
    import numpy as np
    from transformers import pipeline, AutoModelForSeq2SeqLM, AutoTokenizer, AutoModel
    from keybert import KeyBERT
    from sentence_transformers import SentenceTransformer
    from sklearn.metrics.pairwise import cosine_similarity
    ADVANCED_NLP_AVAILABLE = True
except ImportError as e:
    ADVANCED_NLP_AVAILABLE = False
    logging.warning(f"Advanced NLP dependencies not available: {str(e)}")
    logging.warning("Will use basic summarization instead")

from app.services.hungarian_nlp import (
    clean_hungarian_text,
    fix_sentence_boundaries,
    get_hungarian_sentence_importance,
    HUNGARIAN_STOPWORDS
)

# Set up logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# Define the best model for Hungarian summarization
SUMMARIZATION_MODEL = "Helsinki-NLP/opus-mt-en-hu"  # This is for translation, will be replaced with a direct Hungarian model
EMBEDDING_MODEL = "distiluse-base-multilingual-cased-v2"  # Good multilingual model that supports Hungarian

@lru_cache(maxsize=1)
def get_keybert_model():
    """
    Load and cache the KeyBERT model for keyword extraction.
    Uses a multilingual model with good Hungarian support.
    """
    if not ADVANCED_NLP_AVAILABLE:
        return None
        
    try:
        logger.info(f"Loading KeyBERT with model: {EMBEDDING_MODEL}")
        keybert_model = KeyBERT(model=EMBEDDING_MODEL)
        return keybert_model
    except Exception as e:
        logger.error(f"Error loading KeyBERT model: {str(e)}")
        return None

@lru_cache(maxsize=1)
def get_sentence_transformer():
    """Load and cache the sentence transformer model"""
    if not ADVANCED_NLP_AVAILABLE:
        return None
        
    try:
        logger.info(f"Loading SentenceTransformer with model: {EMBEDDING_MODEL}")
        model = SentenceTransformer(EMBEDDING_MODEL)
        return model
    except Exception as e:
        logger.error(f"Error loading SentenceTransformer model: {str(e)}")
        return None

def summarize_text(text: str) -> Tuple[str, List[str]]:
    """
    Summarize the provided text and extract key bullet points.
    
    Args:
        text: The text to summarize
        
    Returns:
        A tuple containing (summary, list_of_bullet_points)
    """
    # Clean and prepare the text
    cleaned_text = clean_hungarian_text(text.strip())
    if not cleaned_text:
        return "No text to summarize.", []
    
    if not ADVANCED_NLP_AVAILABLE:
        logger.warning("Advanced NLP not available, using basic summarization")
        return create_basic_summary(cleaned_text)
    
    try:
        # Extract the most important sentences for an extractive summary
        summary = create_extractive_summary(cleaned_text)
        
        # Extract bullet points using KeyBERT and important sentences
        bullet_points = extract_key_points(cleaned_text)
        
        # If the advanced approach fails, fall back to the basic implementation
        if not summary or not bullet_points:
            logger.warning("Advanced summarization produced empty results, falling back to basic method")
            return create_basic_summary(cleaned_text)
        
        return summary, bullet_points
    
    except Exception as e:
        logger.error(f"Error in advanced summarization: {str(e)}")
        # Fall back to the basic implementation if the advanced one fails
        return create_basic_summary(cleaned_text)

def create_extractive_summary(text: str) -> str:
    """
    Create an extractive summary by selecting the most important sentences.
    
    This method uses a sentence transformer to encode sentences and ranks them
    by their similarity to the document centroid.
    """
    # Split text into sentences
    sentences = re.split(r'(?<=[.!?])\s+', text)
    
    # Fix sentence boundaries (combines fragments that shouldn't be separate)
    sentences = fix_sentence_boundaries(sentences)
    
    if len(sentences) <= 3:
        return text  # If there are only a few sentences, return the original text
    
    if not ADVANCED_NLP_AVAILABLE:
        return " ".join(sentences[:min(3, len(sentences))])
    
    try:
        # Get the sentence transformer model
        model = get_sentence_transformer()
        if not model:
            return " ".join(sentences[:min(3, len(sentences))])
        
        # Encode all sentences to get embeddings
        sentence_embeddings = model.encode(sentences)
        
        # Calculate the centroid (average) of all sentence embeddings
        centroid = sentence_embeddings.mean(axis=0)
        
        # Calculate similarity of each sentence to the centroid
        similarities = []
        for i, embedding in enumerate(sentence_embeddings):
            # Combine embedding similarity with Hungarian-specific importance
            embedding_sim = cosine_similarity(
                embedding.reshape(1, -1), 
                centroid.reshape(1, -1)
            )[0][0]
            
            # Get Hungarian-specific importance score
            hungarian_importance = get_hungarian_sentence_importance(sentences[i])
            
            # Combine scores (70% embedding similarity, 30% linguistic importance)
            combined_score = 0.7 * embedding_sim + 0.3 * hungarian_importance
            similarities.append(combined_score)
        
        # Get indices of top sentences (about 30% of the original)
        num_sentences = max(3, int(len(sentences) * 0.3))
        top_indices = np.argsort(similarities)[-num_sentences:]
        top_indices = sorted(top_indices)  # Sort to maintain original order
        
        # Create summary from selected sentences
        summary_sentences = [sentences[i] for i in top_indices]
        summary = " ".join(summary_sentences)
        
        return summary
    
    except Exception as e:
        logger.error(f"Error in extractive summarization: {str(e)}")
        # Fall back to basic summary if extractive approach fails
        return " ".join(sentences[:min(3, len(sentences))])

def extract_key_points(text: str) -> List[str]:
    """
    Extract key points from the text to create bullet points.
    
    Uses KeyBERT to identify important keywords and phrases, then finds
    sentences containing these keywords.
    """
    # Split text into sentences
    sentences = re.split(r'(?<=[.!?])\s+', text)
    sentences = fix_sentence_boundaries(sentences)
    
    if not ADVANCED_NLP_AVAILABLE:
        return simple_bullet_points(sentences)
    
    # Get KeyBERT model
    keybert_model = get_keybert_model()
    if not keybert_model:
        # Fall back to simple bullet points if KeyBERT isn't available
        return simple_bullet_points(sentences)
    
    try:
        # Extract top keywords and keyphrases
        keywords = keybert_model.extract_keywords(
            text, 
            keyphrase_ngram_range=(1, 2),  # Single words and bigrams
            stop_words=list(HUNGARIAN_STOPWORDS),  # Use Hungarian stopwords
            top_n=10
        )
        
        # Find sentences containing these keywords
        bullet_points = []
        used_sentences = set()
        
        for keyword, score in keywords:
            if len(bullet_points) >= 5:
                break
                
            for sentence in sentences:
                # Check if the sentence contains the keyword and isn't too short
                if (keyword.lower() in sentence.lower() and 
                    len(sentence) > 30 and 
                    sentence not in used_sentences):
                    bullet_points.append(sentence.strip())
                    used_sentences.add(sentence)
                    break
        
        # If we couldn't find enough bullet points, add some of the highest scored sentences
        if len(bullet_points) < 3 and len(sentences) > 3:
            # Calculate importance for each sentence
            importance_scores = [(get_hungarian_sentence_importance(s), s) for s in sentences 
                                if s not in used_sentences and len(s.strip()) > 30]
            importance_scores.sort(reverse=True)
            
            # Add the most important sentences
            for _, sentence in importance_scores[:5 - len(bullet_points)]:
                bullet_points.append(sentence.strip())
        
        return bullet_points
    
    except Exception as e:
        logger.error(f"Error in key point extraction: {str(e)}")
        return simple_bullet_points(sentences)

def simple_bullet_points(sentences: List[str]) -> List[str]:
    """
    Create simple bullet points by selecting sentences from the middle of the text.
    This is a fallback method when the advanced approach fails.
    """
    bullet_points = []
    
    # Skip the first 3 sentences (which are likely in the summary)
    start_idx = min(3, len(sentences) // 4)
    
    # Get some sentences from the middle of the text
    for i in range(start_idx, min(start_idx + 8, len(sentences))):
        if i < len(sentences) and len(sentences[i].strip()) > 30:  # Only use sentences of reasonable length
            bullet_points.append(sentences[i].strip())
            if len(bullet_points) >= 5:
                break
    
    return bullet_points

def create_basic_summary(text: str) -> Tuple[str, List[str]]:
    """
    Basic fallback summary method when advanced approaches fail.
    """
    # Split text into sentences
    sentences = re.split(r'(?<=[.!?])\s+', text)
    sentences = fix_sentence_boundaries(sentences)
    
    # Take first few sentences for summary
    summary_sentences = sentences[:min(3, len(sentences))]
    summary = " ".join(summary_sentences)
    
    # Extract some bullet points
    bullet_points = []
    for i in range(3, min(8, len(sentences))):
        if i < len(sentences) and len(sentences[i].strip()) > 20:  # Only use sentences of reasonable length
            bullet_points.append(sentences[i].strip())
            if len(bullet_points) >= 5:
                break
    
    return summary, bullet_points

# Future implementation will include:
# - Proper Hungarian language text processing
# - Integration with a language model for actual summarization
# - Extract meaningful bullet points using NLP techniques
# - Handle different document types and structures appropriately 