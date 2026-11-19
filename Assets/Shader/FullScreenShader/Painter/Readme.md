## Kuwahara Painter Filter 🎨

A **Kuwahara-based painterly rendering filter** that stylizes an image to resemble a hand-painted artwork. The filter smooths regions while preserving edges, producing a brush-like effect commonly used for non-photorealistic rendering.

### Performance Optimization

Since the **Kuwahara painter shader is computationally expensive**, the filter improves performance using a **downscale → filter → upscale** workflow:

1. Downscale the input image
2. Apply the Kuwahara filter
3. Upscale the filtered result back to the original resolution

The **DownScale** parameter controls this process. Higher values improve performance but may reduce visual quality.
Recommended value: **2**.

### Parameters

**DownScale**
Controls how much the image is reduced before applying the filter.
Higher values increase performance but may degrade the final output quality.

**Sample Steps**
Controls the sampling step size used in the kernel loop. Increasing this value skips samples, reducing the number of texture reads and improving performance.

**Radius**
Defines the size of the Kuwahara sampling region. Larger values create a stronger painterly effect but reduce performance.

### Performance Summary

* **Higher Radius → stronger painter effect, slower performance**
* **Higher DownScale → better performance, lower image fidelity**
* **Higher Sample Steps → fewer samples, faster shader**

In short:
Increasing **DownScale** and **Sample Steps** improves performance, while increasing **Radius** enhances the painterly appearance.

