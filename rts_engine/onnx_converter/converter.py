import tensorflow as tf
import tf2onnx

model = tf.keras.models.load_model("dqn.h5")

input_shape = model.input_shape

spec = (
    tf.TensorSpec(
        input_shape,
        tf.float32,
        name="state"
    ),
)

tf2onnx.convert.from_keras(
    model,
    input_signature=spec,
    output_path="dqn.onnx"
)
